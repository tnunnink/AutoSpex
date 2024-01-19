using L5Sharp.Core;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Persistence.Tests.Sources;

[TestFixture]
public class GetSelectedSourceTests
{
    [Test]
    public async Task GetSelectedSource_NoSourceSelected_ShouldReturnSuccessAndNull()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        
        var result = await mediator.Send(new GetSelectedSource());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }
    
    [Test]
    public async Task GetSelectedSource_ValidSourceExists_ShouldReturnValidResult()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var source = new Source(L5X.Load(Known.Test)) {IsSelected = true};
        await mediator.Send(new CreateSource(source));
        
        var result = await mediator.Send(new GetSelectedSource());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(source, c => c.Excluding(s => s.L5X));
    }
}