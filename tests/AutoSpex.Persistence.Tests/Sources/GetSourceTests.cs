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
    public async Task Send_ValidSourceExists_ShouldReturnValidResult()
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
}