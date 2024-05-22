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
        await mediator.Send(new CreateNode(node));
        var variable = new Variable(node.NodeId, "MyVar", "Test Value");
        await mediator.Send(new SaveVariables([variable]));
        
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
        await mediator.Send(new CreateNode(node));
        var var01 = new Variable(node.NodeId, "Var01", "Test", "This is a test");
        var var02 = new Variable(node.NodeId, "Var02", "Test", "This is a test");
        var var03 = new Variable(node.NodeId, "Var03", "Test", "This is a test");
        await mediator.Send(new SaveVariables([var01, var02, var03]));

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
        var folder = collection.AddContainer();
        var spec = folder.AddSpec();
        await mediator.Send(new CreateNode(collection));
        await mediator.Send(new CreateNode(folder));
        await mediator.Send(new CreateNode(spec));
        var var01 = new Variable(collection.NodeId, "CollectionVar", 123);
        var var02 = new Variable(folder.NodeId, "FolderVar", "Test Value");
        var var03 = new Variable(spec.NodeId, "SpecVar", TagType.Base);
        await mediator.Send(new SaveVariables([var01, var02, var03]));

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
        var folder = collection.AddContainer();
        var spec = folder.AddSpec();
        await mediator.Send(new CreateNode(collection));
        await mediator.Send(new CreateNode(folder));
        await mediator.Send(new CreateNode(spec));
        var var01 = new Variable(collection.NodeId, "MyVar01", 123);
        var var02 = new Variable(folder.NodeId, "MyVar01", "Test Value");
        var var03 = new Variable(spec.NodeId, "MyVar01", TagType.Base);
        await mediator.Send(new SaveVariables([var01, var02, var03]));

        var result = await mediator.Send(new GetScopedVariables(spec.NodeId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().Should().BeEquivalentTo(var03);
        result.Value.First().Value.Should().Be(TagType.Base);
    }
}