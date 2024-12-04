namespace AutoSpex.Persistence.Tests.Nodes;

[TestFixture]
public class LoadNodeTests
{
    [Test]
    public async Task LoadNode_NoNodeExists_ShouldBeFailed()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var result = await mediator.Send(new LoadNode(Guid.NewGuid()));

        result.IsFailed.Should().BeTrue();
    }

    [Test]
    public async Task LoadNode_SeededCollectionNode_ShouldBeSuccessAndExpected()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewCollection();
        await mediator.Send(new CreateNode(node));

        var result = await mediator.Send(new LoadNode(node.NodeId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(node);
    }

    [Test]
    public async Task LoadNode_SeededContainerNode_ShouldBeSuccessAndExpected()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewContainer();
        await mediator.Send(new CreateNode(node));

        var result = await mediator.Send(new LoadNode(node.NodeId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(node);
    }

    [Test]
    public async Task LoadNode_SeededSpecNode_ShouldBeSuccessAndExpected()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewSpec();
        await mediator.Send(new CreateNode(node));

        var result = await mediator.Send(new LoadNode(node.NodeId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(node);
    }

    [Test]
    public async Task LoadNode_ConfiguredSpec_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewSpec("test", s =>
        {
            s.Get(Element.Program)
                .Where("Name", Operation.EqualTo, "SomeName")
                .Validate("Disabled", Operation.EqualTo, false);
        });
        await mediator.Send(new CreateNode(node));

        var result = await mediator.Send(new LoadNode(node.NodeId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(node);
    }
}