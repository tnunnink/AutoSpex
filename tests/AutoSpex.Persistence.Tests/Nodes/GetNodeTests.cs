namespace AutoSpex.Persistence.Tests.Nodes;

[TestFixture]
public class GetNodeTests
{
    [Test]
    public async Task GetNode_ContainerNode_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewContainer("MyContainer");
        await mediator.Send(new CreateNode(node, NodeType.Spec));

        var result = await mediator.Send(new GetNode(node.NodeId));
        
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(node);
    }

    [Test]
    public async Task GetNode_SpecNode_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewSpec("MySpec");
        await mediator.Send(new CreateNode(node));

        var result = await mediator.Send(new GetNode(node.NodeId));
        
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(node);
    }
    
    [Test]
    public async Task GetNode_SourceNode_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewSource("MySource");
        await mediator.Send(new CreateNode(node));

        var result = await mediator.Send(new GetNode(node.NodeId));
        
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(node);
    }
    
    [Test]
    public async Task GetNode_RunNode_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewRun("MyRun");
        await mediator.Send(new CreateNode(node));

        var result = await mediator.Send(new GetNode(node.NodeId));
        
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(node);
    }

    [Test]
    public async Task GetNode_MultipleSeededNodes_ShouldBeAllBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var collection = Node.NewContainer();
        var folder = collection.AddContainer();
        var spec = folder.AddSpec();
        await mediator.Send(new CreateNode(collection, NodeType.Spec));
        await mediator.Send(new CreateNode(folder, NodeType.Spec));
        await mediator.Send(new CreateNode(spec, NodeType.Spec));
        

        var getCollection = await mediator.Send(new GetNode(collection.NodeId));
        getCollection.IsSuccess.Should().BeTrue();
        
        var getFolder = await mediator.Send(new GetNode(folder.NodeId));
        getFolder.IsSuccess.Should().BeTrue();
        
        var getSpec = await mediator.Send(new GetNode(spec.NodeId));
        getSpec.IsSuccess.Should().BeTrue();
    }
}