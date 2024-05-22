namespace AutoSpex.Persistence.Tests.Nodes;

[TestFixture]
public class RenameNodeTests
{
    [Test]
    public async Task RenameNode_NoData_ShouldReturnFailed()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var result = await mediator.Send(new RenameNode(Node.NewContainer()));

        result.IsFailed.Should().BeTrue();
    }
    
    [Test]
    public async Task RenameNode_ContainerNode_ShouldReturnSuccessAndExpectedName()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewContainer();
        await mediator.Send(new CreateNode(node, NodeType.Spec));

        node.Name = "MyContainer";
        var result = await mediator.Send(new RenameNode(node));

        result.IsSuccess.Should().BeTrue();
        var get = await mediator.Send(new GetNode(node.NodeId));
        get.IsSuccess.Should().BeTrue();
        get.Value.Name.Should().Be("MyContainer");
    }
    
    [Test]
    public async Task RenameNode_SpecNode_ShouldReturnSuccessAndExpectedName()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewSpec();
        await mediator.Send(new CreateNode(node));

        node.Name = "MySpec";
        var result = await mediator.Send(new RenameNode(node));

        result.IsSuccess.Should().BeTrue();
        var get = await mediator.Send(new GetNode(node.NodeId));
        get.IsSuccess.Should().BeTrue();
        get.Value.Name.Should().Be("MySpec");
    }
    
    [Test]
    public async Task RenameNode_SourceNode_ShouldReturnSuccessAndExpectedName()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewSource();
        await mediator.Send(new CreateNode(node));

        node.Name = "MySource";
        var result = await mediator.Send(new RenameNode(node));

        result.IsSuccess.Should().BeTrue();
        var get = await mediator.Send(new GetNode(node.NodeId));
        get.IsSuccess.Should().BeTrue();
        get.Value.Name.Should().Be("MySource");
    }
    
    [Test]
    public async Task RenameNode_RunNode_ShouldReturnSuccessAndExpectedName()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewRun();
        await mediator.Send(new CreateNode(node));

        node.Name = "MyRun";
        var result = await mediator.Send(new RenameNode(node));

        result.IsSuccess.Should().BeTrue();
        var get = await mediator.Send(new GetNode(node.NodeId));
        get.IsSuccess.Should().BeTrue();
        get.Value.Name.Should().Be("MyRun");
    }
}