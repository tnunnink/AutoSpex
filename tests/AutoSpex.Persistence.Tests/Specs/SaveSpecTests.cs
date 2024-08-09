namespace AutoSpex.Persistence.Tests.Specs;

[TestFixture]
public class SaveSpecTests
{
    [Test]
    public async Task SaveSpec_DefaultSpec_ShouldBeFailed()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var spec = new Spec();

        var result = await mediator.Send(new SaveSpec(spec));

        result.IsFailed.Should().BeTrue();
    }
    
    [Test]
    public async Task SaveSpec_NoSeededNode_ShouldBeFailed()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var spec = new Spec(Node.NewSpec());

        var result = await mediator.Send(new SaveSpec(spec));

        result.IsFailed.Should().BeTrue();
    }
    
    [Test]
    public async Task SaveSpec_SeededNode_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewSpec();
        await mediator.Send(new CreateNode(node));
        var spec = new Spec(node);

        var result = await mediator.Send(new SaveSpec(spec));

        result.IsSuccess.Should().BeTrue();
    }
    
    [Test]
    public async Task SaveSpec_ConfiguredSpec_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewSpec();
        await mediator.Send(new CreateNode(node));
        
        var spec = new Spec(node)
            .Find(Element.Program)
            .Where(Element.Program.Property("Name"), Operation.EqualTo, "SomeName")
            .ShouldHave(Element.Program.Property("Disabled"), Operation.False);

        var result = await mediator.Send(new SaveSpec(spec));

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task SaveSpec_SpecWithReferences_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewSpec();
        await mediator.Send(new CreateNode(node));
        
        var spec = new Spec(node)
            .Find(Element.Program)
            .Where(Element.Program.Property("Name"), Operation.EqualTo, new Reference("SomeName"))
            .ShouldHave(Element.Program.Property("Disabled"), Operation.False);

        var result = await mediator.Send(new SaveSpec(spec));

        result.IsSuccess.Should().BeTrue();
    }
    
    [Test]
    public async Task SaveSpec_SpecAlreadyExists_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewSpec();
        await mediator.Send(new CreateNode(node));
        var spec = new Spec(node);
        await mediator.Send(new SaveSpec(spec));

        spec.Find(Element.Tag).Where(Element.Tag.Property("Name"), Operation.Containing, "TagName");
        
        var result = await mediator.Send(new SaveSpec(spec));

        result.IsSuccess.Should().BeTrue();
    }
}