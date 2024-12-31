using L5Sharp.Core;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Persistence.Tests.Runs;

[TestFixture]
public class ListRunsForTests
{
    [Test]
    public async Task ListRunsFor_NothingSeeded_ShouldReturnEmpty()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var result = await mediator.Send(new ListRunsFor(Guid.NewGuid()));

        result.Should().BeEmpty();
    }
    
    [Test]
    public async Task ListRunsFor_MultipleEmptyRunsSameId_ShouldReturnExpectedCount()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        await mediator.Send(new PostRun(Run.Empty));
        await mediator.Send(new PostRun(Run.Empty));
        await mediator.Send(new PostRun(Run.Empty));

        var result = await mediator.Send(new ListRunsFor(Guid.Empty));

        result.Should().HaveCount(3);
    }
    
    [Test]
    public async Task LoadRun_MultipleConfiguredRunsTargetId_ShouldReturnSingleItem()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var target = Node.NewSpec("Test");
        await mediator.Send(new PostRun(new Run(target, new Source(L5X.Load(Known.Test)))));
        await mediator.Send(new PostRun(new Run(Node.NewSpec("Test"), new Source(L5X.Load(Known.Test)))));
        await mediator.Send(new PostRun(new Run(Node.NewSpec("Test"), new Source(L5X.Load(Known.Test)))));

        var result = await mediator.Send(new ListRunsFor(target.NodeId));

        result.Should().HaveCount(1);
    }
}