namespace AutoSpex.Persistence.Tests.Variables;

[TestFixture]
public class SaveVariablesTests
{
    [Test]
    public async Task SaveVariables_NoAssignedScope_ShouldReturnFailed()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var variable = new Variable("Var01", "Test");

        var result = await mediator.Send(new SaveVariables(Guid.NewGuid(), new[] { variable }));

        result.IsFailed.Should().BeTrue();
    }

    [Test]
    public async Task SaveVariables_SingleVariableAssignedToNonExistingNode_ShouldReturnFailed()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewContainer();
        var variable = new Variable("Var01", "Test");

        var result = await mediator.Send(new SaveVariables(node.NodeId, [variable]));

        result.IsFailed.Should().BeTrue();
    }

    [Test]
    public async Task SaveVariables_SingleVariableAssignedToExistingNode_ShouldReturnSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewContainer();
        await mediator.Send(new CreateNode(node));
        var variable = new Variable("Var01", "Test");

        var result = await mediator.Send(new SaveVariables(node.NodeId, [variable]));

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task SaveVariables_ManyVariableAssignedToNode_ShouldReturnSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewContainer();
        await mediator.Send(new CreateNode(node));
        var var02 = new Variable("Var02", "Test");
        var var01 = new Variable("Var01", "Test");
        var var03 = new Variable("Var03", "Test");

        var result = await mediator.Send(new SaveVariables(node.NodeId, new[] { var01, var02, var03 }));

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task SaveVariables_ManyVariableAssignedManyNodes_ShouldReturnSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var container = Node.NewContainer();
        await mediator.Send(new CreateNode(container));

        var folder = container.AddContainer();
        await mediator.Send(new CreateNode(folder));

        var var01 = new Variable("Var01", "Test");
        var var02 = new Variable("Var01", "Test");
        var var03 = new Variable("Var02", "Test");

        var result1 = await mediator.Send(new SaveVariables(container.NodeId, [var01]));
        result1.IsSuccess.Should().BeTrue();

        var result2 = await mediator.Send(new SaveVariables(folder.NodeId, [var02, var03]));
        result2.IsSuccess.Should().BeTrue();
    }
}