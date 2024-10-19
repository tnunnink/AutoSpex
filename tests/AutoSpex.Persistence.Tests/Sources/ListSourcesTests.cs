using L5Sharp.Core;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Persistence.Tests.Sources;

[TestFixture]
public class ListSourcesTests
{
    [Test]
    public async Task ListSources_WithoutSeeding_ShouldReturnSingleDefault()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var result = await mediator.Send(new ListSources());

        result.Should().BeEmpty();
    }

    [Test]
    public async Task ListSources_WithSeeding_ShouldReturnExpected()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        await mediator.Send(new CreateSource(new Source(L5X.Load(Known.Test))));
        await mediator.Send(new CreateSource(new Source(L5X.Load(Known.Test))));
        await mediator.Send(new CreateSource(new Source(L5X.Load(Known.Test))));

        var result = await mediator.Send(new ListSources());
        
        result.Should().HaveCount(3);
    }
}