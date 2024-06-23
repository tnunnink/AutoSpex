using L5Sharp.Core;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Persistence.Tests.Runs;

[TestFixture]
public class SaveRunTests
{
    
    [Test]
    public async Task SaveRun_NoExistingNode_ShouldReturnFailed()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var run = new Run();

        var result = await mediator.Send(new SaveRun(run));

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Satisfy(e => e.Message.Contains("Run not found"));
    }
    
    [Test]
    public async Task SaveRun_ExistingNode_ShouldReturnSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewRun();
        await mediator.Send(new CreateNode(node));
        var run = new Run(node);
        
        var result = await mediator.Send(new SaveRun(run));

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task SaveRun_WithSpecNodeOutcome_ShouldReturnSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewRun();
        var spec = Node.NewSpec();
        await mediator.Send(new CreateNode(node));
        await mediator.Send(new CreateNode(spec));
        var run = new Run(node);
        run.AddNode(spec);
        run.Outcomes.Should().NotBeEmpty();
        
        var result = await mediator.Send(new SaveRun(run));

        result.IsSuccess.Should().BeTrue();
    }
    
    [Test]
    public async Task SaveRun_WithSourceNodeOutcome_ShouldReturnSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewRun();
        var source = Node.NewSource();
        await mediator.Send(new CreateNode(node));
        await mediator.Send(new CreateNode(source));
        var run = new Run(node);
        run.AddNode(source);
        run.Outcomes.Should().NotBeEmpty();
        
        var result = await mediator.Send(new SaveRun(run));

        result.IsSuccess.Should().BeTrue();
    }
    
    [Test]
    public async Task SaveRun_WithSpecAndSourceNodeOutcome_ShouldReturnSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewRun();
        var spec = Node.NewSpec();
        var source = Node.NewSource();
        await mediator.Send(new CreateNode(node));
        await mediator.Send(new CreateNode(source));
        await mediator.Send(new CreateNode(spec));
        var run = new Run(node);
        run.AddNode(spec);
        run.AddNode(source);
        run.Outcomes.Should().NotBeEmpty();
        
        var result = await mediator.Send(new SaveRun(run));

        result.IsSuccess.Should().BeTrue();
    }
    
    [Test]
    public async Task SaveRun_WithManySpecAndSourceNodeOutcomes_ShouldReturnSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var node = Node.NewRun();
        var run = new Run(node);
        run.AddNode(Node.NewSpec());
        run.AddNode(Node.NewSpec());
        run.AddNode(Node.NewSpec());
        run.AddNode(Node.NewSource());
        run.AddNode(Node.NewSource());
        run.AddNode(Node.NewSource());
        await mediator.Send(new CreateNode(node));
        foreach (var runNode in run.Nodes)
            await mediator.Send(new CreateNode(runNode));
        
        run.Outcomes.Should().NotBeEmpty();
        
        var result = await mediator.Send(new SaveRun(run));

        result.IsSuccess.Should().BeTrue();
    }
    
    [Test]
    public async Task SaveRun_WithActualEvaluatedOutcomes_ShouldReturnSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        
        var runNode = Node.NewRun();
        var run = new Run(runNode);
        await mediator.Send(new CreateNode(runNode));

        var specNode = Node.NewSpec();
        await mediator.Send(new CreateNode(specNode));
        var spec = new Spec(specNode)
            .Query(Element.Tag)
            .Where(Element.Tag.Property("TagName"), Operation.Contains, "Test")
            .Where(Element.Tag.Property("DataType"), Operation.Equal, "DINT")
            .Verify(Element.Tag.Property("Value"), Operation.GreaterThan, 100);

        var sourceNode = Node.NewSource();
        await mediator.Send(new CreateNode(sourceNode));
        var source = new Source(sourceNode);
        var content = L5X.Load(Known.Test);
        source.Update(content, true);
        
        run.AddNode(specNode);
        run.AddNode(sourceNode);
        run.Outcomes.Should().HaveCount(1);

        await run.Execute(source, [spec]);
        
        var result = await mediator.Send(new SaveRun(run));

        result.IsSuccess.Should().BeTrue();
    }
}