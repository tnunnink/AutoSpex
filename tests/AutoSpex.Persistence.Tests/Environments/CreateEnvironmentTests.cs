using Environment = AutoSpex.Engine.Environment;

namespace AutoSpex.Persistence.Tests.Environments;

[TestFixture]
public class CreateEnvironmentTests
{
    [Test]
    public async Task CreateEnvironment_NewInstance_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var environment = new Environment();

        var result = await mediator.Send(new CreateEnvironment(environment));
        
        result.IsSuccess.Should().BeTrue();
    }
    
    [Test]
    public async Task CreateEnvironment_InstanceWithSource_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var environment = new Environment();
        environment.Add(new Uri(Known.Test));

        var result = await mediator.Send(new CreateEnvironment(environment));
        
        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task CreateEnvironment_ManySources_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var environment = new Environment
        {
            Name = "MyRunner",
            Comment = "This is a test"
        };
        environment.Add(new Uri(Known.Test));
        environment.Add(new Uri(Known.Test));
        environment.Add(new Uri(Known.Test));

        var results = await mediator.Send(new CreateEnvironment(environment));
        
        results.IsSuccess.Should().BeTrue();
    }
}