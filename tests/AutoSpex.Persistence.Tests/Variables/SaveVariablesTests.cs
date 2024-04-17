using AutoSpex.Persistence.Variables;

namespace AutoSpex.Persistence.Tests.Variables;

[TestFixture]
public class SaveVariablesTests
{
    [Test]
    public async Task SaveVariables_NoAssignedScope_ShouldReturnFailed()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var variable = new Variable("Var01", "Test", "This is a test");

        var result = await mediator.Send(new SaveVariables(new[] {variable}));

        result.IsFailed.Should().BeTrue();
    }

    [Test]
    public async Task SaveVariables_SingleVariableAssignedToNonExistingNode_ShouldReturnFailed()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewCollection();
        var variable = new Variable(node.NodeId, "Var01", "Test", "This is a test");

        var result = await mediator.Send(new SaveVariables([variable]));

        result.IsFailed.Should().BeTrue();
    }
    
    [Test]
    public async Task SaveVariables_SingleVariableAssignedToExistingNode_ShouldReturnSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewCollection();
        await mediator.Send(new CreateNode(node));
        var variable = new Variable(node.NodeId, "Var01", "Test", "This is a test");

        var result = await mediator.Send(new SaveVariables([variable]));

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task SaveVariables_ManyVariableAssignedToNode_ShouldReturnSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewCollection();
        await mediator.Send(new CreateNode(node));
        var var02 = new Variable(node.NodeId, "Var02", "Test", "This is a test");
        var var01 = new Variable(node.NodeId, "Var01", "Test", "This is a test");
        var var03 = new Variable(node.NodeId, "Var03", "Test", "This is a test");

        var result = await mediator.Send(new SaveVariables(new[] {var01, var02, var03}));

        result.IsSuccess.Should().BeTrue();
    }
    
    [Test]
    public async Task SaveVariables_ManyVariableAssignedManyNodes_ShouldReturnSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var collection = Node.NewCollection();
        var folder = collection.AddFolder();
        await mediator.Send(new CreateNode(collection));
        await mediator.Send(new CreateNode(folder));
        var var01 = new Variable(collection.NodeId, "Var01");
        var var02 = new Variable(folder.NodeId, "Var01");
        var var03 = new Variable(folder.NodeId, "Var02");

        var result = await mediator.Send(new SaveVariables(new[] {var01, var02, var03}));

        result.IsSuccess.Should().BeTrue();
    }
}