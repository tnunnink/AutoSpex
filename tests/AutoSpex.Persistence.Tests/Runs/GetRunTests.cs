namespace AutoSpex.Persistence.Tests.Runs;

[TestFixture]
public class GetRunTests
{
    [Test]
    public async Task GetRun_NoSpecExists_ShouldBeFailed()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var result = await mediator.Send(new GetRun(Guid.NewGuid()));

        result.IsFailed.Should().BeTrue();
    }

    [Test]
    public async Task GetRun_SeededSpec_ShouldBeSuccessAndExpected()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var run = Node.NewRun();
        await mediator.Send(new CreateNode(run));

        var result = await mediator.Send(new GetRun(run.NodeId));

        result.IsSuccess.Should().BeTrue();
        result.Value.RunId.Should().Be(run.NodeId);
        result.Value.Outcomes.Should().BeEmpty();
        result.Value.Sources.Should().BeEmpty();
        result.Value.Specs.Should().BeEmpty();
    }
    
    [Test]
    public async Task GetRun_WithSpecNodeOutcome_ShouldReturnSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var runNode = Node.NewRun();
        var specNode = Node.NewSpec();
        await mediator.Send(new CreateNode(runNode));
        await mediator.Send(new CreateNode(specNode));
        var run = new Run(runNode);
        run.AddNode(specNode);
        await mediator.Send(new SaveRun(run));

        var result = await mediator.Send(new GetRun(run.RunId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Outcomes.Should().HaveCount(1);
        result.Value.Nodes.Should().HaveCount(1);
        result.Value.Specs.Should().HaveCount(1);
        result.Value.Sources.Should().HaveCount(0);
    }
    
    [Test]
    public async Task GetRun_WithSourceNodeOutcome_ShouldReturnSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var runNode = Node.NewRun();
        var sourceNode = Node.NewSource();
        await mediator.Send(new CreateNode(runNode));
        await mediator.Send(new CreateNode(sourceNode));
        var run = new Run(runNode);
        run.AddNode(sourceNode);
        await mediator.Send(new SaveRun(run));

        var result = await mediator.Send(new GetRun(run.RunId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Outcomes.Should().HaveCount(1);
        result.Value.Nodes.Should().HaveCount(1);
        result.Value.Sources.Should().HaveCount(1);
        result.Value.Specs.Should().HaveCount(0);
    }
    
    [Test]
    public async Task GetRun_WithManySpecAndSourceOutcomes_ShouldReturnSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var runNode = Node.NewRun();
        await mediator.Send(new CreateNode(runNode));
        var run = new Run(runNode);
        run.AddNode(Node.NewSpec());
        run.AddNode(Node.NewSpec());
        run.AddNode(Node.NewSpec());
        run.AddNode(Node.NewSource());
        run.AddNode(Node.NewSource());
        run.AddNode(Node.NewSource());
        foreach (var node in run.Nodes)
            await mediator.Send(new CreateNode(node));
        await mediator.Send(new SaveRun(run));

        var result = await mediator.Send(new GetRun(run.RunId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Outcomes.Should().HaveCount(9);
        result.Value.Nodes.Should().HaveCount(6);
        result.Value.Sources.Should().HaveCount(3);
        result.Value.Specs.Should().HaveCount(3);
    }
}