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
        var spec = new Spec(Guid.NewGuid());

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
        var spec = new Spec(node.NodeId);

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
        
        var spec = new Spec(node.NodeId)
            .Query(Element.Program)
            .Where(Element.Program.Property("Name"), Operation.Equal, "SomeName")
            .Verify(Element.Program.Property("Disabled"), Operation.IsFalse);

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
        var spec = new Spec(node.NodeId);
        await mediator.Send(new SaveSpec(spec));

        spec.Query(Element.Tag).Where(Element.Tag.Property("Name"), Operation.Contains, "TagName");
        
        var result = await mediator.Send(new SaveSpec(spec));

        result.IsSuccess.Should().BeTrue();
    }
}