using Environment = AutoSpex.Engine.Environment;

namespace AutoSpex.Persistence.Tests.Environments;

[TestFixture]
public class GetTargetEnvironmentTests
{
    [Test]
    public async Task TargetEnvironment_NoSeed_ShouldReturnFailed()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var result = await mediator.Send(new GetTargetEnvironment());

        result.IsFailed.Should().BeTrue();
    }

    [Test]
    public async Task TargetEnvironment_Seeded_ShouldReturnSuccessWithDefaultInstance()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var environment = new Environment { IsTarget = true };
        await mediator.Send(new CreateEnvironment(environment));
        await mediator.Send(new TargetEnvironment(environment.EnvironmentId));

        var result = await mediator.Send(new GetTargetEnvironment());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(environment);
    }
}