namespace AutoSpex.Persistence.Tests.Sources;

[TestFixture]
public class TargetSourceTests
{
    [Test]
    public async Task TargetSource_NoSeed_ShouldReturnFailed()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var source = new Source();

        var result = await mediator.Send(new TargetSource(source.SourceId));

        result.IsFailed.Should().BeTrue();
    }

    [Test]
    public async Task TargetSource_Seeded_ShouldReturnSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var source = new Source();
        await mediator.Send(new CreateSource(source));

        var result = await mediator.Send(new TargetSource(source.SourceId));

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task TargetSource_Seeded_ShouldMatchGetTarget()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var source = new Source { IsTarget = true };
        await mediator.Send(new CreateSource(source));

        var result = await mediator.Send(new TargetSource(source.SourceId));
        result.IsSuccess.Should().BeTrue();

        var target = await mediator.Send(new GetTargetSource());
        target.IsSuccess.Should().BeTrue();
        target.Value.SourceId.Should().Be(source.SourceId);
    }

    [Test]
    public async Task TargetSource_ManySeeded_AllOthersShouldNotBeIsTarget()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var target = new Source("Target");
        await mediator.Send(new CreateSource(target));
        await mediator.Send(new CreateSource(new Source("Second")));
        await mediator.Send(new CreateSource(new Source("Thrid")));

        var result = await mediator.Send(new TargetSource(target.SourceId));
        result.IsSuccess.Should().BeTrue();

        var list = await mediator.Send(new ListSources());
        var others = list.Where(e => e.SourceId != target.SourceId).ToList();
        others.Should().AllSatisfy(e => e.IsTarget.Should().BeFalse());
    }
}