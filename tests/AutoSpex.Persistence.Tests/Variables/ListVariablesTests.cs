namespace AutoSpex.Persistence.Tests.Variables;

[TestFixture]
public class ListVariablesTests
{
    [Test]
    public async Task ListVariables_NoExistingVariablesForNode_ShouldBeSuccessAndEmpty()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var result = await mediator.Send(new ListVariables());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Test]
    public async Task ListVariables_ExistingVariablesForNode_ShouldBeSuccessAndNotEmpty()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewContainer();
        await mediator.Send(new CreateNode(node));
        var var01 = new Variable("Var01", "Test");
        var var02 = new Variable("Var02", "Test");
        var var03 = new Variable("Var03", "Test");
        await mediator.Send(new SaveVariables(node.NodeId, new[] { var01, var02, var03 }));

        var result = await mediator.Send(new ListVariables());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
    }
}