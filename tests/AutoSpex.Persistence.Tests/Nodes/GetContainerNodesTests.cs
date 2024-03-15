namespace AutoSpex.Persistence.Tests.Nodes;

[TestFixture]
public class GetContainerNodesTests
{
    [Test]
    public async Task GetContainerNodes_NoData_ShouldBeSuccessAnEmpty()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var result = await mediator.Send(new GetContainerNodes());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Test]
    public async Task GetContainerNodes_WithSeededNodes_ShouldBeSuccessAndExpectedCount()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        await mediator.Send(new CreateNode(Node.NewCollection()));
        await mediator.Send(new CreateNode(Node.NewFolder()));
        await mediator.Send(new CreateNode(Node.NewSpec()));

        var result = await mediator.Send(new GetContainerNodes());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Test]
    public async Task GetContainerNodes_WithSeededHierarchy_ShouldBeSuccessAndExpectedCount()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var collection = Node.NewCollection();
        await mediator.Send(new CreateNode(collection));
        await mediator.Send(new CreateNode(collection.AddFolder()));
        await mediator.Send(new CreateNode(collection.AddSpec()));

        var result = await mediator.Send(new GetContainerNodes());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.First().NodeType.Should().Be(NodeType.Collection);
    }
}