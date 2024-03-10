using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Persistence.Tests.Runners;

[TestFixture]
public class CreateRunnerTests
{
    [Test]
    public async Task CreateRunner_ValidRunner_ShouldReturnSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var runner = new Runner();

        var result = await mediator.Send(new CreateRunner(runner));

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task CreateRunner_ValidRunner_GetRunnerShouldReturnExpected()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var runner = new Runner { Name = "Test", Description = "This is a test runner"};

        await mediator.Send(new CreateRunner(runner));

        var result = await mediator.Send(new GetRunner(runner.RunnerId));
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(runner);
    }

    [Test]
    public async Task CreateRunner_WithNode_ShouldReturnSuccessAndExpected()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var runner = new Runner();
        runner.AddNode(Node.NewCollection("Root"));
        
        await mediator.Send(new CreateRunner(runner));

        var result = await mediator.Send(new GetRunner(runner.RunnerId));
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(runner);
    }
}