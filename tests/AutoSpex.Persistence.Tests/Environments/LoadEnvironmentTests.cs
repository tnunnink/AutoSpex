using L5Sharp.Core;
using Environment = AutoSpex.Engine.Environment;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Persistence.Tests.Environments;

[TestFixture]
public class LoadEnvironmentTests
{
    [Test]
    public async Task GetEnvironment_DoesNotExists_ShouldBeFailure()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var environment = new Environment();

        var result = await mediator.Send(new LoadEnvironment(environment.EnvironmentId));
        
        result.IsFailed.Should().BeTrue();
    }
    
    [Test]
    public async Task GetEnvironment_Seeded_ShouldBeSuccessAndEquivalent()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var environment = new Environment();
        await mediator.Send(new CreateEnvironment(environment));

        var result = await mediator.Send(new LoadEnvironment(environment.EnvironmentId));
        
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(environment);
    }

    [Test]
    public async Task GetEnvironment_WithChanges_ShouldBeSuccessAndExpected()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var environment = new Environment();
        await mediator.Send(new CreateEnvironment(environment));
        environment.Comment = "This is my test environment";
        environment.Add(new Uri(Known.Test));
        await mediator.Send(new SaveEnvironment(environment));

        var result = await mediator.Send(new LoadEnvironment(environment.EnvironmentId));
        
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(environment);
    }
    
    [Test]
    public async Task GetEnvironment_WithSourceWithOverrides_ShouldBeSuccessAndExpected()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        
        //Create node.
        var container = Node.NewContainer();
        await mediator.Send(new CreateNode(container));
       
        //Create variables.
        var var01 = new Variable("TestVar", 123);
        var var02 = new Variable("AnotherVar", Radix.Decimal);
        var var03 = new Variable("ComplexVar", new Tag("Test", new TIMER()));
        await mediator.Send(new SaveVariables(container.NodeId, new[] { var01, var02, var03 }));
        
        //Create environment.
        var environment = new Environment();
        await mediator.Send(new CreateEnvironment(environment));

        //Create source with variables and add to environment.
        var source = new Source(new Uri(Known.Test));
        source.Overrides.AddRange([var01, var02, var03]);
        environment.Sources.Add(source);
        await mediator.Send(new SaveEnvironment(environment));

        var result = await mediator.Send(new LoadEnvironment(environment.EnvironmentId));
        
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(environment);
    }
}