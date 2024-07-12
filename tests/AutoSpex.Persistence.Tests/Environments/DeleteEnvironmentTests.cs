using Environment = AutoSpex.Engine.Environment;

namespace AutoSpex.Persistence.Tests.Environments;

[TestFixture]
public class DeleteEnvironmentTests
{
    [Test]
    public async Task DeleteEnvironment_NonExistingEnvironment_ShouldReturnSuccess()
    {
        var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var environment = Environment.Default;

        var result = await mediator.Send(new DeleteEnvironment(environment.EnvironmentId));

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task DeleteEnvironment_SeededEnvironment_ShouldReturnSuccessAndGettingShouldFail()
    {
        var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var environment = Environment.Default;
        await mediator.Send(new CreateEnvironment(environment));

        var deleted = await mediator.Send(new DeleteEnvironment(environment.EnvironmentId));

        deleted.IsSuccess.Should().BeTrue();
        var get = await mediator.Send(new LoadEnvironment(environment.EnvironmentId));
        get.IsFailed.Should().BeTrue();
    }
}