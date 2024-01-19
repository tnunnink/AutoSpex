using L5Sharp.Core;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Persistence.Tests.Sources;

[TestFixture]
public class DeleteSourceTests
{
    [Test]
    public async Task DeleteSource_NoSource_ShouldReturnFailedWithError()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        
        var result = await mediator.Send(new DeleteSource(Guid.NewGuid()));

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Satisfy(e => e.Message.Contains("Source not found"));
    }
    
    [Test]
    public async Task DeleteSource_SourceExists_ShouldReturnSuccessAndGetSourceReturnsFailure()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var source = new Source(L5X.Load(Known.Test));
        await mediator.Send(new CreateSource(source));
        
        var delete = await mediator.Send(new DeleteSource(source.SourceId));
        delete.IsSuccess.Should().BeTrue();

        var get = await mediator.Send(new GetSource(source.SourceId));
        get.IsFailed.Should().BeTrue();
    }
}