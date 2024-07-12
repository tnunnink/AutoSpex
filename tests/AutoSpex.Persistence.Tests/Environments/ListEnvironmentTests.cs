using Environment = AutoSpex.Engine.Environment;

namespace AutoSpex.Persistence.Tests.Environments;

[TestFixture]
public class ListEnvironmentTests
{
    [Test]
    public async Task ListEnvironments_WithoutSeeding_ShouldReturnSingleDefault()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();

        var result = await mediator.Send(new ListEnvironments());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }
    
    [Test]
    public async Task ListEnvironments_WithSeeding_ShouldReturnExpected()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        await mediator.Send(new CreateEnvironment(Environment.Default));
        await mediator.Send(new CreateEnvironment(Environment.Default));
        await mediator.Send(new CreateEnvironment(Environment.Default));

        var result = await mediator.Send(new ListEnvironments());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(4);
    }
}