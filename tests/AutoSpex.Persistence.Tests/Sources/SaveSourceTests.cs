using L5Sharp.Core;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Persistence.Tests.Sources;

[TestFixture]
public class SaveSourceTests
{
    [Test]
    public async Task SaveSource_NonExistingSource_ShouldReturnFailed()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var source = new Source(L5X.Load(Known.Test));

        var result = await mediator.Send(new SaveSource(source));

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Satisfy(e => e.Message.Contains("Source not found"));
    }

    [Test]
    public async Task SaveSource_ExistingSource_ShouldReturnSuccessAndExpectedUpdates()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var source = new Source();
        await mediator.Send(new CreateSource(source));

        var before = await mediator.Send(new GetSource(source.SourceId));
        before.IsSuccess.Should().BeTrue();
        before.Value.TargetName.Should().BeNullOrEmpty();
        before.Value.TargetType.Should().BeNullOrEmpty();
        before.Value.ExportedBy.Should().BeNullOrEmpty();
        before.Value.Content.Should().BeNullOrEmpty();

        source.Update(L5X.Load(Known.Test), true);
        var update = await mediator.Send(new SaveSource(source));
        update.IsSuccess.Should().BeTrue();

        var result = await mediator.Send(new GetSource(source.SourceId));
        result.IsSuccess.Should().BeTrue();
        result.Value.TargetName.Should().Be("TestController");
        result.Value.TargetType.Should().Be("Controller");
        result.Value.ExportedBy.Should().Be("tnunnink, EN Engineering");
        result.Value.Content.Should().NotBeNullOrEmpty();
    }

    [Test]
    public async Task SaveSource_ReplaceContent_ShouldReturnSuccessAndExpectedUpdates()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var source = new Source(L5X.Load(Known.Example));
        await mediator.Send(new CreateSource(source));

        source.Update(L5X.Load(Known.Test), true);

        var update = await mediator.Send(new SaveSource(source));
        update.IsSuccess.Should().BeTrue();

        var result = await mediator.Send(new GetSource(source.SourceId));
        result.IsSuccess.Should().BeTrue();
        result.Value.TargetName.Should().Be("TestController");
        result.Value.TargetType.Should().Be("Controller");
        result.Value.Content.Should().NotBeEmpty();
    }
}