namespace AutoSpex.Persistence.Tests.Nodes;

[TestFixture]
public class ListNodesTests
{
    [Test]
    public async Task ListNodes_SpecTypeNoData_ShouldBeSuccessAnEmpty()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var result = await mediator.Send(new ListNodes(NodeType.Spec));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Test]
    public async Task ListNodes_SpecTypeWithSeededNodes_ShouldBeSuccessAndExpectedCount()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        await mediator.Send(new CreateNode(Node.NewContainer(), NodeType.Spec));
        await mediator.Send(new CreateNode(Node.NewContainer(), NodeType.Spec));
        await mediator.Send(new CreateNode(Node.NewSpec()));

        var result = await mediator.Send(new ListNodes(NodeType.Spec));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
    }

    [Test]
    public async Task ListNodes_WithSeededHierarchy_ShouldBeSuccessAndExpectedCount()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var collection = Node.NewContainer();
        await mediator.Send(new CreateNode(collection, NodeType.Spec));
        await mediator.Send(new CreateNode(collection.AddContainer(), NodeType.Spec));
        await mediator.Send(new CreateNode(collection.AddSpec()));

        var result = await mediator.Send(new ListNodes(NodeType.Spec));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().Type.Should().Be(NodeType.Container);
        result.Value.First().Nodes.Should().HaveCount(2);
    }
}