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
        var collection = Node.NewContainer();
        await mediator.Send(new CreateNode(collection));
        await mediator.Send(new CreateNode(collection.AddContainer()));
        var destination = collection.AddContainer();
        await mediator.Send(new CreateNode(destination));
        await mediator.Send(new CreateNode(collection.AddContainer()));
        var target = collection.AddSpec(); //store target for move
        await mediator.Send(new CreateNode(target));
        await mediator.Send(new CreateNode(collection.AddSpec()));
        
        //Will update the parent node for the target correctly.
        destination.AddNode(target);
        
        var result = await mediator.Send(new MoveNode(target));
        
        result.IsSuccess.Should().BeTrue();
    }
}