using L5Sharp.Core;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Persistence.Tests.Sources;

[TestFixture]
public class RenameSourceTests
{
    [Test]
    public async Task UpdateSource_NonExistingSource_ShouldReturnFailed()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var source = new Source(L5X.Load(Known.Test));
        
        var result = await mediator.Send(new RenameSource(source));

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Satisfy(e => e.Message.Contains("Source not found"));
    }

    [Test]
    public async Task UpdateSource_ExistingSource_ShouldReturnSuccessAndExpectedUpdates()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var source = new Source(L5X.Load(Known.Test));
        await mediator.Send(new CreateSource(source));

        source.Name = "Renamed Source";
        
        var update = await mediator.Send(new RenameSource(source));
        update.IsSuccess.Should().BeTrue();

        var result = await mediator.Send(new GetSource(source.SourceId));
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Renamed Source");
    }
}