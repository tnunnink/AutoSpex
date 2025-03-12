using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Persistence.Tests.Sources;

[TestFixture]
public class UpdateSourceTests
{
    [Test]
    public async Task SaveSource_NotSeeded_ShouldBeBecauseNoneExists()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var source = new Source();

        var result = await mediator.Send(new UpdateSource(source));

        result.IsFailed.Should().BeTrue();
    }

    [Test]
    public async Task SaveSource_SeededNoChanges_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var source = new Source();
        await mediator.Send(new CreateSource(source));

        var result = await mediator.Send(new UpdateSource(source));

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task SaveSource_SeededWithSource_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var source = new Source();
        await mediator.Send(new CreateSource(source));

        var result = await mediator.Send(new UpdateSource(source));
        result.IsSuccess.Should().BeTrue();

        var get = await mediator.Send(new LoadSource(source.SourceId));
        get.IsSuccess.Should().BeTrue();
        get.Value.Should().BeEquivalentTo(source, o => o.Excluding(s => s.Content));
    }
}