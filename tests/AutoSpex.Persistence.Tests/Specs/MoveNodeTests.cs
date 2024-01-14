namespace AutoSpex.Persistence.Tests.Specs;

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
        await mediator.Send(new CreateNode(collection.AddFolder()));
        await mediator.Send(new CreateNode(collection.AddSpec()));
        await mediator.Send(new CreateNode(collection.AddFolder()));
        var target = collection.AddSpec(); //store target for move
        await mediator.Send(new CreateNode(target));
        await mediator.Send(new CreateNode(collection.AddSpec()));
        await mediator.Send(new CreateNode(collection.AddFolder()));
        
        //maybe this should return node but idk
        collection.InsertNode(target, 1);
        var result = await mediator.Send(new MoveNode(target));
        
        result.IsSuccess.Should().BeTrue();
    }
}