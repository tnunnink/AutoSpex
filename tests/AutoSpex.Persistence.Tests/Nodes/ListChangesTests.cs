namespace AutoSpex.Persistence.Tests.Nodes;

[TestFixture]
public class ListChangesTests
{
    [Test]
    public async Task ListChanges_NoData_ShouldBeSuccessAnEmpty()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var result = await mediator.Send(new ListChanges(Guid.NewGuid()));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Test]
    public async Task ListChanges_SingleSeededNode_ShouldBeSuccessAndExpectedCount()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var container = Node.NewContainer();
        await mediator.Send(new CreateNode(container));

        var result = await mediator.Send(new ListChanges(container.NodeId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    [Test]
    public async Task ListChanges_ManySeededNodes_ShouldBeSuccessAndExpectedCount()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var container = Node.NewContainer();
        var nested = container.AddContainer();
        var spec = nested.AddSpec();
        await mediator.Send(new CreateNode(container));
        await mediator.Send(new CreateNode(nested));
        await mediator.Send(new CreateNode(spec));

        var result = await mediator.Send(new ListChanges(container.NodeId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }
}