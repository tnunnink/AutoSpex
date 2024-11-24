using L5Sharp.Core;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Persistence.Tests.Sources;

[TestFixture]
public class SaveSourceTests
{
    [Test]
    public async Task SaveSource_NotSeeded_ShouldBeBecauseNoneExists()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var source = new Source();

        var result = await mediator.Send(new SaveSource(source));

        result.IsFailed.Should().BeTrue();
    }

    [Test]
    public async Task SaveSource_SeededNoChanges_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var source = new Source();
        await mediator.Send(new CreateSource(source));

        var result = await mediator.Send(new SaveSource(source));

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task SaveSource_SeededWithSource_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var source = new Source();
        await mediator.Send(new CreateSource(source));

        var result = await mediator.Send(new SaveSource(source));
        result.IsSuccess.Should().BeTrue();

        var get = await mediator.Send(new LoadSource(source.SourceId));
        get.IsSuccess.Should().BeTrue();
        get.Value.Should().BeEquivalentTo(source, o => o.Excluding(s => s.Content));
    }
    
    [Test]
    public async Task SaveSource_SeededSourceWithSuppressions_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        //Create nodes.
        var container = Node.NewContainer();
        var spec01 = container.AddSpec();
        var spec02 = container.AddSpec();
        var spec03 = container.AddSpec();
        await mediator.Send(new CreateNodes([container, spec01, spec02, spec03]));

        //Create source.
        var source = new Source();
        await mediator.Send(new CreateSource(source));

        //Add overrides to source.
        source.AddSuppression(spec01.NodeId, "Just to test that this works");
        source.AddSuppression(spec02.NodeId, "Just to test that this works");
        source.AddSuppression(spec03.NodeId, "Just to test that this works");

        var result = await mediator.Send(new SaveSource(source));
        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task SaveSource_SeededWithSourceWithOverrides_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        //Create node.
        var container = Node.NewContainer();
        var spec01 = container.AddSpec("Test", s =>
        {
            s.Fetch(Element.Tag);
            s.Where("TagName", Operation.Containing, "Something");
            s.Confirm("Value", Operation.EqualTo, 12);
        });
        var spec02 = container.AddSpec("Test", s =>
        {
            s.Fetch(Element.Tag);
            s.Where("TagName", Operation.Containing, "Something");
            s.Confirm("Value", Operation.EqualTo, 12);
        });
        var spec03 = container.AddSpec("Test", s =>
        {
            s.Fetch(Element.Tag);
            s.Where("TagName", Operation.Containing, "Something");
            s.Confirm("Value", Operation.EqualTo, 12);
        });
        await mediator.Send(new CreateNodes([container, spec01, spec02, spec03]));

        //Create source.
        var source = new Source();
        await mediator.Send(new CreateSource(source));

        //Add overrides to source.
        source.AddOverride(spec01);
        source.AddOverride(spec01);
        source.AddOverride(spec01);

        var result = await mediator.Send(new SaveSource(source));
        result.IsSuccess.Should().BeTrue();
    }
}