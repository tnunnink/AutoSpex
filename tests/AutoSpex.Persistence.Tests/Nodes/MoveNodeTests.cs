namespace AutoSpex.Persistence.Tests.Nodes;

[TestFixture]
public class MoveNodeTests
{
    [Test]
    public async Task MoveNode_SameParentDifferentOrdinal_ShouldReturnSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        
        //seed some nodes
        var collection = Node.NewCollection();
        await mediator.Send(new CreateNode(collection));
        await mediator.Send(new CreateNode(collection.AddFolder()));
        var destination = collection.AddFolder();
        await mediator.Send(new CreateNode(destination));
        await mediator.Send(new CreateNode(collection.AddFolder()));
        var target = collection.AddSpec(); //store target for move
        await mediator.Send(new CreateNode(target));
        await mediator.Send(new CreateNode(collection.AddSpec()));
        
        //Will update the parent node for the target correctly.
        destination.AddNode(target);
        
        var result = await mediator.Send(new MoveNode(target));
        
        result.IsSuccess.Should().BeTrue();
    }
}