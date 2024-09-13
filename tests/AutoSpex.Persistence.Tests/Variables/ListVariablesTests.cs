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
        node.AddVariable("Var01", "Test");
        node.AddVariable("Var02", "Test");
        node.AddVariable("Var03", "Test");
        await mediator.Send(new CreateNode(node));

        var result = await mediator.Send(new ListVariables());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
    }
}