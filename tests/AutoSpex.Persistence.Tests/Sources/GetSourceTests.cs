using L5Sharp.Core;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Persistence.Tests.Sources;

[TestFixture]
public class GetSourceTests
{
    [Test]
    public async Task GetSource_NoSource_ShouldReturnFailed()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        
        var result = await mediator.Send(new GetSource(Guid.NewGuid()));

        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Contain("Source not found");
    }
    
    [Test]
    public async Task GetSource_ValidSourceExists_ShouldReturnEquivalent()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var source = new Source(L5X.Load(Known.Test));
        await mediator.Send(new CreateSource(source));
        
        var result = await mediator.Send(new GetSource(source.SourceId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(source, c => c.Excluding(s => s.L5X));
    }
    
    [Test]
    public async Task GetSource_ValidSourceExists_ShouldHaveNonNullContent()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var source = new Source(L5X.Load(Known.Test));
        await mediator.Send(new CreateSource(source));
        
        var result = await mediator.Send(new GetSource(source.SourceId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Content.Should().NotBeNull();
        result.Value.L5X.Should().NotBeNull();
    }
    [Test]
    public async Task GetSource_NoContentSource_ShouldHaveNullContent()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var source = new Source();
        await mediator.Send(new CreateSource(source));
        
        var result = await mediator.Send(new GetSource(source.SourceId));

        result.IsSuccess.Should().BeTrue();
        result.Value.TargetName.Should().BeNullOrEmpty();
        result.Value.TargetType.Should().BeNullOrEmpty();
        result.Value.ExportedBy.Should().BeNullOrEmpty();
        result.Value.ExportedOn.Should().BeWithin(TimeSpan.FromSeconds(1));
        result.Value.Content.Should().BeNullOrEmpty();
    }
    
}