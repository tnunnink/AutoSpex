using L5Sharp.Core;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Persistence.Tests.Runs;

[TestFixture]
public class ListRunsTests
{
    [Test]
    public async Task ListRuns_NothingSeeded_ShouldReturnEmpty()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var result = await mediator.Send(new ListRuns());

        result.Should().BeEmpty();
    }
    
    [Test]
    public async Task LoadRun_MultipleEmptyRuns_ShouldReturnExpectedCount()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        await mediator.Send(new PostRun(Run.Empty));
        await mediator.Send(new PostRun(Run.Empty));
        await mediator.Send(new PostRun(Run.Empty));

        var result = await mediator.Send(new ListRuns());

        result.Should().HaveCount(3);
    }

    [Test]
    public async Task LoadRun_MultipleConfiguredRuns_ShouldReturnEquivalentRun()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        await mediator.Send(new PostRun(new Run(Node.NewSpec("Test"), new Source(L5X.Load(Known.Test)))));
        await mediator.Send(new PostRun(new Run(Node.NewSpec("Test"), new Source(L5X.Load(Known.Test)))));
        await mediator.Send(new PostRun(new Run(Node.NewSpec("Test"), new Source(L5X.Load(Known.Test)))));

        var result = await mediator.Send(new ListRuns());

        result.Should().HaveCount(3);
    }
}