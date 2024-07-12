using L5Sharp.Core;
using Argument = AutoSpex.Engine.Argument;
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
        var variable = new Variable("MyVar", "Test Value");
        await mediator.Send(new SaveVariables(node.NodeId, [variable]));

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
        var var01 = new Variable("Var01", "Test");
        var var02 = new Variable("Var02", "Test");
        var var03 = new Variable("Var03", "Test");
        await mediator.Send(new SaveVariables(node.NodeId, [var01, var02, var03]));

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
        await mediator.Send(new SaveVariables(collection.NodeId, [new Variable("CollectionVar", 123)]));
        await mediator.Send(new SaveVariables(folder.NodeId, [new Variable("FolderVar", "Test Value")]));
        await mediator.Send(new SaveVariables(spec.NodeId, [new Variable("SpecVar", TagType.Base)]));

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
        await mediator.Send(new SaveVariables(collection.NodeId, [new Variable("MyVar01", 123)]));
        await mediator.Send(new SaveVariables(folder.NodeId, [new Variable("MyVar01", "Test Value")]));
        await mediator.Send(new SaveVariables(spec.NodeId, [new Variable("MyVar01", TagType.Base)]));

        var result = await mediator.Send(new GetScopedVariables(spec.NodeId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().Name.Should().Be("MyVar01");
        result.Value.First().Type.Should().Be(typeof(TagType));
        result.Value.First().Group.Should().Be(TypeGroup.Enum);
        result.Value.First().Value.Should().Be(TagType.Base);
    }
}