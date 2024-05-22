using L5Sharp.Core;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Persistence.Tests.Sources;

[TestFixture]
public class ListSourcesTests
{
    [Test]
    public async Task ListSources_NoSources_ShouldReturnSuccessAndEmpty()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        
        var result = await mediator.Send(new ListSources());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Test]
    public async Task ListSources_SeededSources_ShouldReturnSuccessAndExpectedCount()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var content = L5X.Load(Known.Test);
        var source1 = new Source(content) {Name = "TestSource1"};
        var source2 = new Source(content) {Name = "TestSource2"};
        var source3 = new Source(content) {Name = "TestSource3"};
        await mediator.Send(new CreateSource(source1));
        await mediator.Send(new CreateSource(source2));
        await mediator.Send(new CreateSource(source3));

        var result = await mediator.Send(new ListSources());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
    }
}