namespace AutoSpex.Persistence.Tests.Nodes;

[TestFixture]
public class LoadAllNodesTests
{
    [Test]
    public async Task LoadAllNodes_SpecTypeNoData_ShouldBeSuccessAnEmpty()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var result = await mediator.Send(new LoadAllNodes());

        result.Should().BeEmpty();
    }

    [Test]
    public async Task LoadAllNodes_SpecTypeWithSeededNodes_ShouldHaveExpectedCount()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        await mediator.Send(new CreateNode(Node.NewSpec()));
        await mediator.Send(new CreateNode(Node.NewSpec()));
        await mediator.Send(new CreateNode(Node.NewSpec()));

        var result = await mediator.Send(new LoadAllNodes());

        result.Should().HaveCount(3);
    }

    [Test]
    public async Task LoadAllNodes_WithSeededHierarchy_ShouldHAveExpectedCount()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var collection = Node.NewContainer();
        await mediator.Send(new CreateNode(collection));
        await mediator.Send(new CreateNode(collection.AddContainer()));
        await mediator.Send(new CreateNode(collection.AddSpec()));

        var result = (await mediator.Send(new LoadAllNodes())).ToList();
        
        result.Should().HaveCount(1);
    }
}