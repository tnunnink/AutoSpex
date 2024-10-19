namespace AutoSpex.Persistence.Tests.Sources;

[TestFixture]
public class GetTargetSourceTests
{
    [Test]
    public async Task TargetSource_NoSeed_ShouldReturnFailed()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var result = await mediator.Send(new GetTargetSource());

        result.IsFailed.Should().BeTrue();
    }

    [Test]
    public async Task TargetSource_Seeded_ShouldReturnSuccessWithDefaultInstance()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var source = new Source { IsTarget = true };
        await mediator.Send(new CreateSource(source));
        await mediator.Send(new TargetSource(source.SourceId));

        var result = await mediator.Send(new GetTargetSource());

        result.IsSuccess.Should().BeTrue();
        result.Value.SourceId.Should().Be(source.SourceId);
    }
}