using Environment = AutoSpex.Engine.Environment;

namespace AutoSpex.Persistence.Tests.Environments;

[TestFixture]
public class RenameEnvironmentTests
{
    [Test]
    public async Task RenameEnvironment_NoData_ShouldReturnFailed()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var environment = new Environment();

        var result = await mediator.Send(new RenameEnvironment(environment));

        result.IsFailed.Should().BeTrue();
    }
    
    [Test]
    public async Task RenameEnvironment_SeededEnvironment_ShouldReturnSuccessAndExpectedName()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var environment = new Environment();
        await mediator.Send(new CreateEnvironment(environment));

        environment.Name = "NewName";
        
        var result = await mediator.Send(new RenameEnvironment(environment));
        result.IsSuccess.Should().BeTrue();
        
        var get = await mediator.Send(new LoadEnvironment(environment.EnvironmentId));
        get.IsSuccess.Should().BeTrue();
        get.Value.Name.Should().Be("NewName");
    }
}