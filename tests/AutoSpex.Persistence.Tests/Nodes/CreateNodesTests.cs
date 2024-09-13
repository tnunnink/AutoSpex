namespace AutoSpex.Persistence.Tests.Nodes;

[TestFixture]
public class CreateNodesTests
{
    [Test]
    public async Task CreateNodes_EmptyCollection_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var result = await mediator.Send(new CreateNodes([]));

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task CreateNodes_SingleNode_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var result = await mediator.Send(new CreateNodes([Node.NewCollection()]));

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task CreateNodes_MultipleNodes_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var result = await mediator.Send(new CreateNodes([Node.NewCollection(), Node.NewContainer(), Node.NewSpec()]));

        result.IsSuccess.Should().BeTrue();
    }
}