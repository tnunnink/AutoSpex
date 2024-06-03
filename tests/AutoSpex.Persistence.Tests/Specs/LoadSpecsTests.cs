namespace AutoSpex.Persistence.Tests.Specs;

[TestFixture]
public class LoadSpecsTests
{
    [Test]
    public async Task LoadSpecs_NoSpecExists_ShouldBePassedAndEmpty()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var result = await mediator.Send(new LoadSpecs([Guid.NewGuid()]));

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

        var result = await mediator.Send(new LoadSpecs([spec01.NodeId, spec02.NodeId, spec03.NodeId]));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
    }

    [Test]
    public async Task LoadSpecs_SeededSpecWithVariableUpdateValue_ShouldBeSuccessAndHaveCorrectVariableValue()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        //Create spec node
        var node = Node.NewSpec();
        await mediator.Send(new CreateNode(node));
        //Create variable to reference node
        var variable = new Variable("MyVar", "SomeValue");
        await mediator.Send(new SaveVariables(node.NodeId, [variable]));
        //Create spec that uses variable
        var spec = new Spec(node.NodeId, node.Name);
        spec.Query(Element.Tag);
        spec.Where("Test", Operation.Contains, variable);
        spec.Verify("Name", Operation.Equal, variable);
        await mediator.Send(new SaveSpec(spec));
        //Update the variable value to ensure the resolving works
        variable.Value = "MostRecentValue";
        await mediator.Send(new SaveVariables(node.NodeId, [variable]));

        //Finally...
        var result = await mediator.Send(new LoadSpecs([node.NodeId]));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);

        var target = result.Value.First();
        target.Filters[0].Arguments[0].As<Argument>().Value.As<Variable>().Value.Should().Be("MostRecentValue");
        target.Verifications[0].Arguments[0].As<Argument>().Value.As<Variable>().Value.Should().Be("MostRecentValue");
    }
}