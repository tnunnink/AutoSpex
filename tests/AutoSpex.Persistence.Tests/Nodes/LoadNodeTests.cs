namespace AutoSpex.Persistence.Tests.Nodes;

[TestFixture]
public class LoadNodeTests
{
    [Test]
    public async Task LoadSpecs_NoSpecExists_ShouldBeFailedResult()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var result = await mediator.Send(new LoadNode(Guid.NewGuid()));

        result.IsFailed.Should().BeTrue();
    }

    [Test]
    public async Task LoadSpecs_SeededSpecNoConfig_ShouldBeSuccessAndExpected()
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
    public async Task LoadNodes_TreeWithSeveralSpecs_ShouldBeSuccessAndExpectedCount()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var collection = Node.NewCollection();
        var container = collection.AddContainer();
        var spec01 = container.AddSpec();
        var spec02 = container.AddSpec();
        var spec03 = container.AddSpec();
        await mediator.Send(new CreateNode(collection));
        await mediator.Send(new CreateNode(container));
        await mediator.Send(new CreateNode(spec01));
        await mediator.Send(new CreateNode(spec02));
        await mediator.Send(new CreateNode(spec03));

        var result = await mediator.Send(new LoadNode(collection.NodeId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(collection);
        result.Value.Descendants().Should().HaveCount(4);
    }

    [Test]
    public async Task LoadNodes_NodeWithSpecsConfigured_ShouldBeSuccessAndExpected()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewSpec("Test");
        node.Specify(c =>
        {
            c.Query(Element.Tag);
            c.Where("TagName", Operation.Containing, "SomeValue");
            c.Verify("Comment", Operation.EqualTo, "SomeValue");
        });
        await mediator.Send(new CreateNode(node));

        var result = await mediator.Send(new LoadNode(node.NodeId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Spec.Should().NotBeNull();
        result.Value.Spec.Should().BeEquivalentTo(node.Spec);
    }
}