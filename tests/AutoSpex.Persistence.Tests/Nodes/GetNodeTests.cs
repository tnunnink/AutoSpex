namespace AutoSpex.Persistence.Tests.Nodes;

[TestFixture]
public class GetNodeTests
{
    [Test]
    public async Task GetNode_NonExistent_ShouldBeFailed()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var result = await mediator.Send(new GetNode(Guid.NewGuid()));
        
        result.IsFailed.Should().BeTrue();
    }
    
    [Test]
    public async Task GetNode_ContainerNode_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewContainer("MyContainer");
        await mediator.Send(new CreateNode(node));

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
    public async Task GetNode_MultipleSeededNodes_ShouldBeAllBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var collection = Node.NewContainer();
        var container = collection.AddContainer();
        var spec = container.AddSpec();
        await mediator.Send(new CreateNodes([collection, container, spec]));
        
        var getCollection = await mediator.Send(new GetNode(collection.NodeId));
        getCollection.IsSuccess.Should().BeTrue();

        var getFolder = await mediator.Send(new GetNode(container.NodeId));
        getFolder.IsSuccess.Should().BeTrue();

        var getSpec = await mediator.Send(new GetNode(spec.NodeId));
        getSpec.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task GetNode_Heirarchy_CollectionShouldHaveAllChildren()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var collection = Node.NewContainer();
        var container = collection.AddContainer();
        var spec = container.AddSpec();
        await mediator.Send(new CreateNodes([collection, container, spec]));

        var result = await mediator.Send(new GetNode(collection.NodeId));

        result.Value.Descendants().Should().HaveCount(2);
    }
    
    [Test]
    public async Task GetNode_Heirarchy_ContainerShouldHaveParentAndChild()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var collection = Node.NewContainer();
        var container = collection.AddContainer();
        var spec = container.AddSpec();
        await mediator.Send(new CreateNodes([collection, container, spec]));

        var result = await mediator.Send(new GetNode(container.NodeId));

        result.Value.Parent.Should().NotBeNull();
        result.Value.Nodes.Should().HaveCount(1);
    }
    
    [Test]
    public async Task GetNode_Heirarchy_SpecShouldHaveAllAncestors()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var collection = Node.NewContainer();
        var folder = collection.AddContainer();
        var spec = folder.AddSpec();
        await mediator.Send(new CreateNodes([collection, folder, spec]));

        var result = await mediator.Send(new GetNode(spec.NodeId));

        result.Value.Ancestors().Should().HaveCount(2);
    }
}