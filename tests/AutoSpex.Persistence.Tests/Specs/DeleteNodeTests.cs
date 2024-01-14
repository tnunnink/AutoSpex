namespace AutoSpex.Persistence.Tests.Specs;

[TestFixture]
public class DeleteNodeTests
{
    [Test]
    public async Task Send_NonExistingNode_ShouldReturnSuccess()
    {
        var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var result = await mediator.Send(new DeleteNode(Guid.NewGuid()));

        result.IsSuccess.Should().BeTrue();
    }
    
    [Test]
    public async Task Send_ExistingNode_ShouldReturnSuccess()
    {
        var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewCollection();
        await mediator.Send(new CreateNode(node));

        var result = await mediator.Send(new DeleteNode(node.NodeId));

        result.IsSuccess.Should().BeTrue();
    }
}