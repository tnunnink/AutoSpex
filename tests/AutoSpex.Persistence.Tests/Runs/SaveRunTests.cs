using L5Sharp.Core;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Persistence.Tests.Runs;

[TestFixture]
public class SaveRunTests
{
    [Test]
    public async Task SaveRun_EmptyRun_ShouldReturnSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var run = Run.Empty;

        var result = await mediator.Send(new SaveRun(run));

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task SaveRun_ConfiguredRun_ShouldReturnSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var run = new Run(Node.NewCollection(), new Source(L5X.Load(Known.Test)));

        var result = await mediator.Send(new SaveRun(run));

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task SaveRun_ConfiguredRun_GetRunShouldReturnEquivalentRun()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var run = new Run(Node.NewCollection(), new Source(L5X.Load(Known.Test)));
        await mediator.Send(new SaveRun(run));

        var result = await mediator.Send(new LoadRun(run.RunId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(run, o => o.Excluding(r => r.Source.Content));
    }

    [Test]
    public async Task SaveRun_WithOutcomes_ShouldReturnSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var collection = Node.NewCollection();
        collection.AddSpec("Spec1");
        collection.AddSpec("Spec2");
        collection.AddSpec("Spec3");
        collection.AddContainer().AddSpec("Nested");
        var run = new Run(collection, new Source(L5X.Load(Known.Test)));

        var result = await mediator.Send(new SaveRun(run));

        result.IsSuccess.Should().BeTrue();
    }
}