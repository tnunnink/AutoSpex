using L5Sharp.Core;
using Task = System.Threading.Tasks.Task;

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
        var node = Node.NewContainer();
        node.AddVariable("MyVar", "Test Value");
        await mediator.Send(new CreateNode(node));

        var result = await mediator.Send(new GetScopedVariables(node.NodeId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    [Test]
    public async Task GetScopedVariables_SeededManyVariablesForSingleNodes_ShouldReturnSuccessAndExpectedCount()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewContainer();
        node.AddVariable("Var01", "Test");
        node.AddVariable("Var02", "Test");
        node.AddVariable("Var03", "Test");
        await mediator.Send(new CreateNode(node));

        var result = await mediator.Send(new GetScopedVariables(node.NodeId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
    }

    [Test]
    public async Task GetScopedVariables_SeededVariablesForManyNodes_ShouldReturnSuccessAndExpectedCount()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var collection = Node.NewContainer();
        collection.AddVariable("CollectionVar", 123);
        var folder = collection.AddContainer();
        folder.AddVariable("FolderVar", "Test Value");
        var spec = folder.AddSpec();
        spec.AddVariable("SpecVar", TagType.Base);
        await mediator.Send(new CreateNode(collection));
        await mediator.Send(new CreateNode(folder));
        await mediator.Send(new CreateNode(spec));

        var result = await mediator.Send(new GetScopedVariables(spec.NodeId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
    }

    [Test]
    public async Task GetScopedVariables_SeededSameNameVariableForDifferentNodes_ShouldReturnSuccessAndExpectedCount()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var collection = Node.NewContainer();
        collection.AddVariable("MyVar01", 123);
        var folder = collection.AddContainer();
        folder.AddVariable("MyVar01", "Test Value");
        var spec = folder.AddSpec();
        spec.AddVariable("MyVar01", TagType.Base);
        await mediator.Send(new CreateNode(collection));
        await mediator.Send(new CreateNode(folder));
        await mediator.Send(new CreateNode(spec));

        var result = await mediator.Send(new GetScopedVariables(spec.NodeId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().Name.Should().Be("MyVar01");
        result.Value.First().Type.Should().Be(typeof(TagType));
        result.Value.First().Group.Should().Be(TypeGroup.Enum);
        result.Value.First().Value.Should().Be(TagType.Base);
    }
}