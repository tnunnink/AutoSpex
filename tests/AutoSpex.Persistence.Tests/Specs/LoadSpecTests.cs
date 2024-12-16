namespace AutoSpex.Persistence.Tests.Specs;

[TestFixture]
public class LoadSpecTests
{
    [Test]
    public async Task LoadSpec_NoNodeExists_ShouldBeFailed()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var result = await mediator.Send(new LoadSpec(Guid.NewGuid()));

        result.IsFailed.Should().BeTrue();
    }

    [Test]
    public async Task LoadSpec_SeededCollectionNode_ShouldBeSuccessAndExpected()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewCollection();
        await mediator.Send(new CreateNode(node));

        var result = await mediator.Send(new LoadSpec(node.NodeId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(node.Spec);
    }

    [Test]
    public async Task LoadSpec_SeededContainerNode_ShouldBeSuccessAndExpected()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewContainer();
        await mediator.Send(new CreateNode(node));

        var result = await mediator.Send(new LoadSpec(node.NodeId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(node.Spec);
    }

    [Test]
    public async Task LoadSpec_SeededSpecNode_ShouldBeSuccessAndExpected()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewSpec();
        await mediator.Send(new CreateNode(node));

        var result = await mediator.Send(new LoadSpec(node.NodeId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(node.Spec);
    }

    [Test]
    public async Task LoadSpec_ConfiguredSpec_ShouldBeSuccess()
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

        var result = await mediator.Send(new LoadSpec(node.NodeId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(node.Spec);
    }
}