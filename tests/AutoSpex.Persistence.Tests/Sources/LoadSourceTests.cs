﻿using L5Sharp.Core;
using Source = AutoSpex.Engine.Source;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Persistence.Tests.Sources;

[TestFixture]
public class LoadSourceTests
{
    [Test]
    public async Task LoadSource_DoesNotExists_ShouldBeFailure()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var result = await mediator.Send(new LoadSource(Guid.NewGuid()));

        result.IsFailed.Should().BeTrue();
    }

    [Test]
    public async Task LoadSource_SeededEmptySource_ShouldBeSuccessAndEquivalent()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var source = new Source();
        await mediator.Send(new CreateSource(source));

        var result = await mediator.Send(new LoadSource(source.SourceId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(source, o => o.Excluding(s => s.Content));
    }
    
    [Test]
    public async Task LoadSource_SeededTestSource_ShouldBeSuccessAndEquivalent()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var source = new Source(L5X.Load(Known.Test));
        await mediator.Send(new CreateSource(source));

        var result = await mediator.Send(new LoadSource(source.SourceId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(source, o => o.Excluding(s => s.Content));
    }
    
    [Test]
    public async Task LoadSource_WithSourceWithSuppressions_ShouldBeSuccessAndExpected()
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
        source.AddSuppression(spec01.NodeId, "Just to test that this works");
        source.AddSuppression(spec02.NodeId, "Just to test that this works");
        source.AddSuppression(spec03.NodeId, "Just to test that this works");
        await mediator.Send(new SaveSource(source));

        var result = await mediator.Send(new LoadSource(source.SourceId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(source, o => o.Excluding(s => s.Content));
    }

    /*[Test]
    public async Task LoadSource_WithSourceWithOverrides_ShouldBeSuccessAndExpected()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        //Create node.
        var container = Node.NewContainer();
        var var01 = container.AddVariable("TestVar", 123);
        var var02 = container.AddVariable("AnotherVar", Radix.Decimal);
        var var03 = container.AddVariable("ComplexVar", new Tag("Test", new TIMER()));
        await mediator.Send(new CreateNode(container));

        //Create source.
        var source = new Source();
        await mediator.Send(new CreateSource(source));
        source.AddOverride(var01);
        source.AddOverride(var02);
        source.AddOverride(var03);
        await mediator.Send(new SaveSource(source));

        var result = await mediator.Send(new LoadSource(source.SourceId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(source, o => o.Excluding(s => s.Content));
    }*/
}