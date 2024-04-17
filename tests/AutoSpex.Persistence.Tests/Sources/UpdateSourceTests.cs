using L5Sharp.Core;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Persistence.Tests.Sources;

[TestFixture]
public class UpdateSourceTests
{
    [Test]
    public async Task UpdateSource_NonExistingSource_ShouldReturnFailed()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var source = new Source(L5X.Load(Known.Test));
        
        var result = await mediator.Send(new UpdateSource(source));

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
        source.Documentation = "This source is a file for a project";
        source.IsSelected = true;
        
        var update = await mediator.Send(new UpdateSource(source));
        update.IsSuccess.Should().BeTrue();

        var result = await mediator.Send(new GetSource(source.SourceId));
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Renamed Source");
        result.Value.Documentation.Should().Be("This source is a file for a project");
        result.Value.IsSelected.Should().BeTrue();
    }

    [Test]
    public async Task UpdateSource_ReplaceContent_ShouldReturnSuccessAndExpectedUpdates()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var source = new Source(L5X.Load(Known.Example));
        await mediator.Send(new CreateSource(source));

        source.Update(L5X.Load(Known.Test));
        
        var update = await mediator.Send(new UpdateSource(source));
        update.IsSuccess.Should().BeTrue();

        var result = await mediator.Send(new GetSource(source.SourceId));
        result.IsSuccess.Should().BeTrue();
        result.Value.TargetName.Should().Be("TestController");
        result.Value.TargetType.Should().Be("Controller");
        result.Value.Content.Should().NotBeEmpty();
    }
}