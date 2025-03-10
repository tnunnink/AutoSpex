﻿using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Persistence.Tests.Specs;

[TestFixture]
public class SaveSpecTests
{
    [Test]
    public async Task SaveSpec_NoExistingNode_ShouldBeFailed()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var spec = Node.NewSpec();

        var result = await mediator.Send(new SaveSpec(spec));

        result.IsFailed.Should().BeTrue();
    }

    [Test]
    public async Task SaveSpec_SeededNodeNoChanges_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewSpec();
        await mediator.Send(new CreateNode(node));

        var result = await mediator.Send(new SaveSpec(node));

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task SaveSpec_ConfiguredSpec_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewSpec();
        await mediator.Send(new CreateNode(node));

        node.Configure(c =>
        {
            c.Get(Element.Program);
            c.Where("Name", Operation.EqualTo, "SomeName");
            c.Validate("Disabled", Operation.EqualTo, false);
        });


        var result = await mediator.Send(new SaveSpec(node));

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task SaveNode_SpecWithReferences_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewSpec();
        await mediator.Send(new CreateNode(node));

        node.Configure(c =>
        {
            c.Get(Element.Program);
            c.Where("Name", Operation.EqualTo, "SomeName");
            c.Validate("Disabled", Operation.EqualTo, false);
        });

        var result = await mediator.Send(new SaveSpec(node));

        result.IsSuccess.Should().BeTrue();
    }
}