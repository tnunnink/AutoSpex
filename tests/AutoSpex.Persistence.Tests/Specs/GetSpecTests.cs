namespace AutoSpex.Persistence.Tests.Specs;

[TestFixture]
public class GetSpecTests
{
    [Test]
    public async Task GetSpec_NoSpecExists_ShouldBeFailed()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var result = await mediator.Send(new GetSpec(Guid.NewGuid()));

        result.IsFailed.Should().BeTrue();
    }
    
    [Test]
    public async Task GetSpec_SeededSpec_ShouldBeSuccessAndExpected()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewSpec();
        await mediator.Send(new CreateNode(node));
        var spec = new Spec(node);
        await mediator.Send(new SaveSpec(spec));

        var result = await mediator.Send(new GetSpec(spec.SpecId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(spec);
    }
}