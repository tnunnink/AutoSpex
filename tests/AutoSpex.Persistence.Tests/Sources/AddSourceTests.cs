using FluentAssertions.Equivalency;
using L5Sharp.Core;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Persistence.Tests.Sources;

[TestFixture]
public class CreateSourceTests
{
    [Test]
    public async Task CreateSource_ValidSource_ShouldReturnSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var source = new Source(L5X.Load(Known.Test));

        var result = await mediator.Send(new CreateSource(source));

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task CreateSource_ValidSource_GetSourceShouldReturnExpected()
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
    public async Task CreateSource_WithSelectedTrue_ShouldReturnSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var source = new Source(L5X.Load(Known.Test)) {IsSelected = true};

        var result = await mediator.Send(new CreateSource(source));

        result.IsSuccess.Should().BeTrue();
    }
    
    [Test]
    public async Task CreateSource_MultipleSourcesWithSelected_ShouldOverrideOtherSelectedSources()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var content = L5X.Load(Known.Test);
        var source1 = new Source(content) {Name = "TestSource1", IsSelected = true};
        var source2 = new Source(content) {Name = "TestSource2", IsSelected = true};
        var source3 = new Source(content) {Name = "TestSource3", IsSelected = true};
        
        await mediator.Send(new CreateSource(source1));
        await mediator.Send(new CreateSource(source2));
        await mediator.Send(new CreateSource(source3));

        var result = await mediator.Send(new ListSources());

        result.Value.Should().HaveCount(3);
        var list = result.Value.ToList();
        list[0].IsSelected.Should().BeFalse();
        list[1].IsSelected.Should().BeFalse();
        list[2].IsSelected.Should().BeFalse();
    }
}