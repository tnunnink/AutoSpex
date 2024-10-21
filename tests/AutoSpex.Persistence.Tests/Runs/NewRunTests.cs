using L5Sharp.Core;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Persistence.Tests.Runs;

[TestFixture]
public class NewRunTests
{
    [Test]
    public async Task NewRun_NoNodeOrSource_ShouldReturnFailed()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var result = await mediator.Send(new NewRun(Guid.NewGuid(), Guid.NewGuid()));

        result.IsFailed.Should().BeTrue();
    }

    [Test]
    public async Task NewRun_NodeDoesNotExist_ShouldReturnFailed()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var source = new Source(L5X.Load(Known.Test));
        await mediator.Send(new CreateSource(source));

        var result = await mediator.Send(new NewRun(Guid.NewGuid(), source.SourceId));

        result.IsFailed.Should().BeTrue();
    }

    [Test]
    public async Task NewRun_SourceDoesNotExist_ShouldReturnFailed()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewSpec();
        await mediator.Send(new CreateNode(node));

        var result = await mediator.Send(new NewRun(node.NodeId, Guid.NewGuid()));

        result.IsFailed.Should().BeTrue();
    }

    [Test]
    public async Task NewRun_HasNodeAndSource_ShouldReturnSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewSpec();
        await mediator.Send(new CreateNode(node));
        var source = new Source(L5X.Load(Known.Test));
        await mediator.Send(new CreateSource(source));

        var result = await mediator.Send(new NewRun(node.NodeId, source.SourceId));

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task NewRun_HasNodeAndSource_ShouldHaveExpectedNodeAndSource()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewSpec("MyNewSpec");
        await mediator.Send(new CreateNode(node));
        var source = new Source(L5X.Load(Known.Test));
        await mediator.Send(new CreateSource(source));

        var result = await mediator.Send(new NewRun(node.NodeId, source.SourceId));

        result.Value.Node.Should().BeEquivalentTo(node);
        result.Value.Source.Should()
            .BeEquivalentTo(source, o => o.Excluding(s => s.Content).Excluding(s => s.IsTarget));
        result.Value.Outcomes.Should().HaveCount(1);
    }

    [Test]
    public async Task NewRun_ContainerWithMultipleSpecs_ShouldHaveExpectedOutcomeCount()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewCollection();
        var spec1 = node.AddSpec();
        var container = node.AddContainer();
        var spec2 = container.AddSpec();
        var spec3 = container.AddSpec();
        await mediator.Send(new CreateNodes([node, container, spec1, spec2, spec3]));
        var source = new Source(L5X.Load(Known.Test));
        await mediator.Send(new CreateSource(source));

        var result = await mediator.Send(new NewRun(node.NodeId, source.SourceId));

        result.Value.Outcomes.Should().HaveCount(3);
    }
}