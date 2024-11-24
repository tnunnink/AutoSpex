namespace AutoSpex.Persistence.Tests.Nodes;

[TestFixture]
public class LoadNodesTests
{
    [Test]
    public async Task LoadSpecs_NoSpecExists_ShouldBePassedAndEmpty()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var result = await mediator.Send(new LoadNodes([Guid.NewGuid()]));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Test]
    public async Task LoadSpecs_SeededSpecsNoVariables_ShouldBeSuccessAndExpected()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var spec01 = Node.NewSpec();
        var spec02 = Node.NewSpec();
        var spec03 = Node.NewSpec();
        await mediator.Send(new CreateNode(spec01));
        await mediator.Send(new CreateNode(spec02));
        await mediator.Send(new CreateNode(spec03));

        var result = await mediator.Send(new LoadNodes([spec01.NodeId, spec02.NodeId, spec03.NodeId]));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
    }

    [Test]
    public async Task LoadNodes_TreeWithSeveralSpecs_ShouldBeSuccessAndExpectedCountAndHaveParents()
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

        var result = await mediator.Send(new LoadNodes([spec01.NodeId, spec02.NodeId, spec03.NodeId]));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
        result.Value.Should().AllSatisfy(node => node.Ancestors().Should().HaveCount(2));
    }

    [Test]
    public async Task LoadNodes_NodeWithSpecsConfiged_ShouldBeSuccessAndExpected()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewSpec("Test");
        node.Configure(c =>
        {
            c.Fetch(Element.Tag);
            c.Where("TagName", Operation.Containing, "SomeValue");
            c.Confirm("Comment", Operation.EqualTo, "SomeValue");
        });
        await mediator.Send(new CreateNode(node));

        var result = await mediator.Send(new LoadNodes([node.NodeId]));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().Spec.Should().BeEquivalentTo(node.Spec);
    }
}