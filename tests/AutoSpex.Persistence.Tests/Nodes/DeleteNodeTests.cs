namespace AutoSpex.Persistence.Tests.Nodes;

[TestFixture]
public class DeleteNodeTests
{
    [Test]
    public async Task DeleteNode_NonExistingNode_ShouldReturnSuccess()
    {
        var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var result = await mediator.Send(new DeleteNode(Guid.NewGuid()));

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task DeleteNode_ContainerNode_ShouldReturnSuccess()
    {
        var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewContainer();
        await mediator.Send(new CreateNode(node));

        var deleted = await mediator.Send(new DeleteNode(node.NodeId));

        deleted.IsSuccess.Should().BeTrue();
        var get = await mediator.Send(new GetNode(node.NodeId));
        get.IsFailed.Should().BeTrue();
    }

    [Test]
    public async Task DeleteNode_SpecNode_ShouldReturnSuccess()
    {
        var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewSpec();
        await mediator.Send(new CreateNode(node));

        var deleted = await mediator.Send(new DeleteNode(node.NodeId));

        deleted.IsSuccess.Should().BeTrue();
        var get = await mediator.Send(new GetNode(node.NodeId));
        get.IsFailed.Should().BeTrue();
    }
}