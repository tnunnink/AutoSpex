using L5Sharp.Core;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Persistence.Tests.Environments;

using Environment = Engine.Environment;

[TestFixture]
public class SaveEnvironmentTests
{
    [Test]
    public async Task SaveEnvironment_NotSeeded_ShouldBeBecauseNoneExists()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var environment = new Environment();

        var result = await mediator.Send(new SaveEnvironment(environment));

        result.IsFailed.Should().BeTrue();
    }

    [Test]
    public async Task SaveEnvironment_SeededNoChanges_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var environment = new Environment();
        await mediator.Send(new CreateEnvironment(environment));

        var result = await mediator.Send(new SaveEnvironment(environment));

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task SaveEnvironment_SeededWithSource_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var environment = new Environment();
        await mediator.Send(new CreateEnvironment(environment));
        
        environment.Comment = "This is my test environment";
        environment.Add(new Uri(Known.Test));

        var result = await mediator.Send(new SaveEnvironment(environment));
        result.IsSuccess.Should().BeTrue();

        var get = await mediator.Send(new LoadEnvironment(environment.EnvironmentId));
        get.IsSuccess.Should().BeTrue();
        get.Value.Should().BeEquivalentTo(environment);
    }

    [Test]
    public async Task SaveEnvironment_SeededWithSourceWithOverrides_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        
        //Create node.
        var container = Node.NewContainer();
        var var01 = container.AddVariable("TestVar", 123);
        var var02 = container.AddVariable("AnotherVar", Radix.Decimal);
        var var03 = container.AddVariable("ComplexVar", new Tag("Test", new TIMER()));
        await mediator.Send(new CreateNode(container));
        
        //Create environment.
        var environment = new Environment();
        await mediator.Send(new CreateEnvironment(environment));

        //Create source with variables and add to environment.
        var source = new Source(new Uri(Known.Test));
        source.Overrides.AddRange([var01, var02, var03]);
        environment.Sources.Add(source);

        var result = await mediator.Send(new SaveEnvironment(environment));
        result.IsSuccess.Should().BeTrue();
    }
}