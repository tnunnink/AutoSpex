using AutoSpex.Persistence.Variables;

namespace AutoSpex.Persistence.Tests.Variables;

[TestFixture]
public class GetScopedVariablesTests
{
    [Test]
    public async Task GetScopedVariables_NoVariablesAtAll_ShouldReturnSuccessAndEmpty()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var result = await mediator.Send(new GetScopedVariables(Guid.Empty));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Test]
    public async Task GetScopedVariables_SeededVariablesForSingleNode_ShouldReturnSuccessAndExpectedCount()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewCollection();
        node.AddVariable("MyVar", "Test Value");
        await mediator.Send(new SaveNode(node));

        var result = await mediator.Send(new GetScopedVariables(node.NodeId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    [Test]
    public async Task GetScopedVariables_SeededManyVariablesForSingleNodes_ShouldReturnSuccessAndExpectedCount()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var node = Node.NewCollection();
        node.AddVariable("MyVar01", "Test Value");
        node.AddVariable("MyVar02", "Test Value");
        node.AddVariable("MyVar03", "Test Value");
        await mediator.Send(new SaveNode(node));

        var result = await mediator.Send(new GetScopedVariables(node.NodeId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
    }

    [Test]
    public async Task GetScopedVariables_SeededVariablesForManyNodes_ShouldReturnSuccessAndExpectedCount()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var collection = Node.NewCollection();
        collection.AddVariable("CollectionVar", "Test Value");
        var folder = collection.AddFolder();
        folder.AddVariable("FolderVar", "Test Value");
        var spec = folder.AddSpec();
        spec.AddVariable("SpecVar", "Test Value");
        await mediator.Send(new SaveNode(collection));
        await mediator.Send(new SaveNode(folder));
        await mediator.Send(new SaveNode(spec));

        var result = await mediator.Send(new GetScopedVariables(spec.NodeId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
    }

    [Test]
    public async Task GetScopedVariables_SeededSameNameVariableForDifferentNodes_ShouldReturnSuccessAndExpectedCount()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var node = Node.NewCollection();
        node.AddVariable("MyVar01", "Test Value");
        await mediator.Send(new SaveNode(node));

        var spec = node.AddSpec();
        var expected = spec.AddVariable("MyVar01", "A more local value");
        await mediator.Send(new SaveNode(spec));

        var result = await mediator.Send(new GetScopedVariables(spec.NodeId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().Should().BeEquivalentTo(expected);
        result.Value.First().Value.Should().Be("A more local value");
    }
}