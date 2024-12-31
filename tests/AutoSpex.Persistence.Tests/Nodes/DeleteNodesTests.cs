namespace AutoSpex.Persistence.Tests.Nodes;

[TestFixture]
public class DeleteNodesTests
{
    [Test]
    public async Task DeleteNodes_SingleNonExistingNode_ShouldReturnSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var result = await mediator.Send(new DeleteNodes([Node.NewCollection()]));

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task DeleteNodes_SingleContainerNode_ShouldReturnSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewContainer();
        await mediator.Send(new CreateNode(node));

        var deleted = await mediator.Send(new DeleteNodes([node]));

        deleted.IsSuccess.Should().BeTrue();
        var get = await mediator.Send(new GetNode(node.NodeId));
        get.IsFailed.Should().BeTrue();
    }

    [Test]
    public async Task DeleteNodes_SingleSpecNode_ShouldReturnSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewSpec();
        await mediator.Send(new CreateNode(node));

        var deleted = await mediator.Send(new DeleteNodes([node]));

        deleted.IsSuccess.Should().BeTrue();
        var get = await mediator.Send(new GetNode(node.NodeId));
        get.IsFailed.Should().BeTrue();
    }
}