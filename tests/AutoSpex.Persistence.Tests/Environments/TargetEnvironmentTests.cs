using Environment = AutoSpex.Engine.Environment;

namespace AutoSpex.Persistence.Tests.Environments;

[TestFixture]
public class TargetEnvironmentTests
{
    [Test]
    public async Task TargetEnvironment_NoSeed_ShouldReturnFailed()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var environment = new Environment();

        var result = await mediator.Send(new TargetEnvironment(environment.EnvironmentId));

        result.IsFailed.Should().BeTrue();
    }
    
    [Test]
    public async Task TargetEnvironment_Seeded_ShouldReturnSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var environment = new Environment();
        await mediator.Send(new CreateEnvironment(environment));

        var result = await mediator.Send(new TargetEnvironment(environment.EnvironmentId));

        result.IsSuccess.Should().BeTrue();
    }
    
    [Test]
    public async Task TargetEnvironment_Seeded_ShouldMatchGetTarget()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var environment = new Environment {IsTarget = true};
        await mediator.Send(new CreateEnvironment(environment));

        var result = await mediator.Send(new TargetEnvironment(environment.EnvironmentId));
        result.IsSuccess.Should().BeTrue();

        var target = await mediator.Send(new GetTargetEnvironment());
        target.IsSuccess.Should().BeTrue();
        target.Value.Should().BeEquivalentTo(environment);
    }
    
    [Test]
    public async Task TargetEnvironment_ManySeeded_AllOthersShouldNotBeIsTarget()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var target = new Environment();
        await mediator.Send(new CreateEnvironment(target));
        await mediator.Send(new CreateEnvironment(new Environment()));
        await mediator.Send(new CreateEnvironment(new Environment()));

        var result = await mediator.Send(new TargetEnvironment(target.EnvironmentId));
        result.IsSuccess.Should().BeTrue();

        var list = await mediator.Send(new ListEnvironments());
        var others = list.Value.Where(e => e.EnvironmentId != target.EnvironmentId).ToList();
        others.Should().AllSatisfy(e => e.IsTarget.Should().BeFalse());
    }
}