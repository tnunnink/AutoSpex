namespace AutoSpex.Persistence.Tests.Environments;

using Environment = Engine.Environment;

[TestFixture]
public class DeleteEnvironmentsTests
{
    [Test]
    public async Task DeleteEnvironments_NonExistingEnvironments_ShouldReturnSuccess()
    {
        var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var environment = new Environment();

        var result = await mediator.Send(new DeleteEnvironments([environment.EnvironmentId]));

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task DeleteEnvironments_SeededEnvironments_ShouldReturnSuccessAndGettingShouldFail()
    {
        var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var first = new Environment();
        var second = new Environment();
        var third = new Environment();
        await mediator.Send(new CreateEnvironment(first));
        await mediator.Send(new CreateEnvironment(second));
        await mediator.Send(new CreateEnvironment(third));

        var deleted = await mediator.Send(new DeleteEnvironments(
        [
            first.EnvironmentId, second.EnvironmentId, third.EnvironmentId
        ]));

        deleted.IsSuccess.Should().BeTrue();
    }
}