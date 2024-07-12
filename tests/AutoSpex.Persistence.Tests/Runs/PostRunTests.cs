using Environment = AutoSpex.Engine.Environment;

namespace AutoSpex.Persistence.Tests.Runs;

[TestFixture]
public class PostRunTests
{
    [Test]
    public async Task PostRun_DefaultInstance_ShouldBeFailedBecauseOfEnvironmentConstraint()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var run = new Run();

        var result = await mediator.Send(new PostRun(run));

        result.IsFailed.Should().BeTrue();
    }
    
    [Test]
    public async Task PostRun_ValidEnvironment_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var run = new Run();
        await mediator.Send(new CreateEnvironment(run.Environment));

        var result = await mediator.Send(new PostRun(run));

        result.IsSuccess.Should().BeTrue();
    }
    
    [Test]
    public async Task PostRun_ValidEnvironmentWithOutcomesNotRun_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var run = new Run();
        await mediator.Send(new CreateEnvironment(run.Environment));
        run.AddNode(Node.NewSpec());
        run.AddNode(Node.NewSpec());
        run.AddNode(Node.NewSpec());

        var result = await mediator.Send(new PostRun(run));

        result.IsSuccess.Should().BeTrue();
    }
    
    [Test]
    public async Task PostRun_AfterExecutionWithResults_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var environment = new Environment();
        environment.Add(new Uri(Known.Test));
        var spec = new Spec();
        spec.Search(Element.DataType)
            .Where(Element.DataType.Property("Name"), Operation.Equal, "ComplexType")
            .ShouldHave(Element.DataType.Property("Members"), Operation.Any,
                new Criterion(Element.DataTypeMember.Property("DataType"), Operation.Equal, "SimpleType"));
        var run = new Run(environment);
        run.AddNode(spec.ToNode());
        await run.ExecuteAsync([spec]);
        await mediator.Send(new CreateEnvironment(run.Environment));

        var result = await mediator.Send(new PostRun(run));

        result.IsSuccess.Should().BeTrue();
    }
}