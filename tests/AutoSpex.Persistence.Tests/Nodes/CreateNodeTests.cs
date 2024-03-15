namespace AutoSpex.Persistence.Tests.Nodes;

[TestFixture]
public class CreateNodeTests
{
    [Test]
    public async Task AddNode_CollectionNode_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewCollection("MyCollection");

        var result = await mediator.Send(new CreateNode(node));
        
        result.IsSuccess.Should().BeTrue();
    }
    
    [Test]
    public async Task AddNode_FolderNode_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewFolder();

        var result = await mediator.Send(new CreateNode(node));
        
        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task AddNode_SpecNode_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewSpec("MySpec");
        
        var result = await mediator.Send(new CreateNode(node));

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task AddNodes_MultipleNodes_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var collection = Node.NewCollection();
        var folder = collection.AddFolder();
        var spec = folder.AddSpec();

        var collectionResult = await mediator.Send(new CreateNode(collection));
        collectionResult.IsSuccess.Should().BeTrue();
        
        var folderResult = await mediator.Send(new CreateNode(folder));
        folderResult.IsSuccess.Should().BeTrue();
        
        var specResult = await mediator.Send(new CreateNode(spec));
        specResult.IsSuccess.Should().BeTrue();
    }
}