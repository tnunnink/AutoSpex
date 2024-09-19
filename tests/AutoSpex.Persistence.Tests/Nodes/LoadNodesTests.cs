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
    public async Task LoadNodes_NodeWithSeveralSpecsConfiged_ShouldBeSuccessAndExpectedCount()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewSpec("Test");
        node.AddSpec(c =>
        {
            c.Find(Element.Tag);
            c.Filter("TagName", Operation.Containing, "SomeValue");
            c.Verify("Comment", Operation.EqualTo, "SomeValue");
        });
        node.AddSpec(c =>
        {
            c.Find(Element.Tag);
            c.Filter("TagName", Operation.Containing, "SomeValue");
            c.Verify("Comment", Operation.EqualTo, "SomeValue");
        });
        node.AddSpec(c =>
        {
            c.Find(Element.Tag);
            c.Filter("TagName", Operation.Containing, "SomeValue");
            c.Verify("Comment", Operation.EqualTo, "SomeValue");
        });

        await mediator.Send(new CreateNode(node));

        var result = await mediator.Send(new LoadNodes([node.NodeId]));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().Specs.Should().HaveCount(3);
    }

    [Test]
    public async Task LoadSpecs_SeededSpecWithVariableUpdateValue_ShouldBeSuccessAndHaveCorrectVariableValue()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var node = Node.NewSpec("Test");
        var variable = node.AddVariable("MyVar", "SomeValue");
        var reference = variable.Reference();
        node.AddSpec(c =>
        {
            c.Find(Element.Tag);
            c.Filter("TagName", Operation.Containing, reference);
            c.Verify("Comment", Operation.EqualTo, reference);
        });
        await mediator.Send(new CreateNode(node));

        variable.Value = "MostRecentValue";
        await mediator.Send(new SaveNode(node));

        var result = await mediator.Send(new LoadNodes([node.NodeId]));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);

        var targetSpec = result.Value.First().Specs.First();

        targetSpec.Filters[0].Arguments[0]
            .As<Argument>().Value
            .As<Reference>()
            .Should()
            .BeEquivalentTo(reference, o => o.Excluding(r => r.Value));

        targetSpec.Verifications[0].Arguments[0]
            .As<Argument>().Value
            .As<Reference>()
            .Should().BeEquivalentTo(reference, o => o.Excluding(r => r.Value));
    }
}