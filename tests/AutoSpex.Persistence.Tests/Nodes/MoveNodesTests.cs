namespace AutoSpex.Persistence.Tests.Nodes;

[TestFixture]
public class MoveNodesTests
{
    [Test]
    public async Task MoveNodes_FromCollectionToContainer_ShouldReturnSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var collection = Node.NewContainer();
        var destination = collection.AddContainer();
        var target = collection.AddSpec(); //store target for move
        await mediator.Send(new CreateNodes([collection, destination, target]));
        
        var result = await mediator.Send(new MoveNodes([target], destination.NodeId));
        
        result.IsSuccess.Should().BeTrue();
    }
}