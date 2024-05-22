namespace AutoSpex.Persistence.Tests.Nodes;

[TestFixture]
public class CreateNodeTests
{
    [Test]
    public async Task CreateNode_ContainerNodeNoType_ShouldBeFailed()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewContainer("Test");

        var result = await mediator.Send(new CreateNode(node));
        
        result.IsFailed.Should().BeTrue();
    }
    
    [Test]
    public async Task CreateNode_ContainerNodeWithTypeSpec_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewContainer("Test");

        var result = await mediator.Send(new CreateNode(node, NodeType.Spec));
        
        result.IsSuccess.Should().BeTrue();
        node.Parent.Should().NotBeNull();
        node.ParentId.Should().NotBeEmpty();
    }
    
    [Test]
    public async Task CreateNode_ContainerNodeWithTypeSource_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewContainer("Test");

        var result = await mediator.Send(new CreateNode(node, NodeType.Source));
        
        result.IsSuccess.Should().BeTrue();
        node.Parent.Should().NotBeNull();
        node.ParentId.Should().NotBeEmpty();
    }
    
    [Test]
    public async Task CreateNode_ContainerNodeWithTypeRun_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewContainer("Test");

        var result = await mediator.Send(new CreateNode(node, NodeType.Run));
        
        result.IsSuccess.Should().BeTrue();
        node.Parent.Should().NotBeNull();
        node.ParentId.Should().NotBeEmpty();
    }

    [Test]
    public async Task CreateNode_SpecNode_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewSpec("MySpec");
        
        var result = await mediator.Send(new CreateNode(node));

        result.IsSuccess.Should().BeTrue();
    }
    
    [Test]
    public async Task CreateNode_SourceNode_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewSource("MySource");
        
        var result = await mediator.Send(new CreateNode(node));

        result.IsSuccess.Should().BeTrue();
    }
    
    [Test]
    public async Task CreateNode_RunNode_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewRun("MyRun");
        
        var result = await mediator.Send(new CreateNode(node));

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task CreateNode_MultipleNodes_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var collection = Node.NewContainer();
        var folder = collection.AddContainer();
        var spec = folder.AddSpec();

        var collectionResult = await mediator.Send(new CreateNode(collection, NodeType.Spec));
        collectionResult.IsSuccess.Should().BeTrue();
        
        var folderResult = await mediator.Send(new CreateNode(folder, NodeType.Spec));
        folderResult.IsSuccess.Should().BeTrue();
        
        var specResult = await mediator.Send(new CreateNode(spec));
        specResult.IsSuccess.Should().BeTrue();
    }
}