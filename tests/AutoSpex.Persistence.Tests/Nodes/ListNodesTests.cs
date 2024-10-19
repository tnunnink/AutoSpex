namespace AutoSpex.Persistence.Tests.Nodes;

[TestFixture]
public class ListNodesTests
{
    [Test]
    public async Task ListNodes_SpecTypeNoData_ShouldBeSuccessAnEmpty()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var result = await mediator.Send(new ListNodes());

        result.Should().BeEmpty();
    }

    [Test]
    public async Task ListNodes_SpecTypeWithSeededNodes_ShouldHaveExpectedCount()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        await mediator.Send(new CreateNode(Node.NewContainer()));
        await mediator.Send(new CreateNode(Node.NewContainer()));
        await mediator.Send(new CreateNode(Node.NewSpec()));

        var result = await mediator.Send(new ListNodes());

        result.Should().HaveCount(3);
    }

    [Test]
    public async Task ListNodes_WithSeededHierarchy_ShouldHAveExpectedCount()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var collection = Node.NewContainer();
        await mediator.Send(new CreateNode(collection));
        await mediator.Send(new CreateNode(collection.AddContainer()));
        await mediator.Send(new CreateNode(collection.AddSpec()));

        var result = (await mediator.Send(new ListNodes())).ToList();
        
        result.Should().HaveCount(1);
        result.First().Type.Should().Be(NodeType.Container);
        result.First().Nodes.Should().HaveCount(2);
    }
}