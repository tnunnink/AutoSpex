using L5Sharp.Core;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Persistence.Tests.Runs;

[TestFixture]
public class LoadRunTests
{
    [Test]
    public async Task LoadRun_EmptyRun_ShouldReturnSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var run = Run.Empty;
        await mediator.Send(new SaveRun(run));

        var result = await mediator.Send(new LoadRun(run.RunId));

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task LoadRun_ConfiguredRun_ShouldReturnSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var run = new Run(Node.NewSpec("Test"), new Source(L5X.Load(Known.Test)));
        await mediator.Send(new SaveRun(run));

        var result = await mediator.Send(new LoadRun(run.RunId));

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task LoadRun_ConfiguredRun_ShouldReturnEquivalentRun()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var run = new Run(Node.NewSpec("Test"), new Source(L5X.Load(Known.Test)));
        await mediator.Send(new SaveRun(run));

        var result = await mediator.Send(new LoadRun(run.RunId));

        result.Value.Should().BeEquivalentTo(run, o => o.Excluding(r => r.Source.Content));
    }
}