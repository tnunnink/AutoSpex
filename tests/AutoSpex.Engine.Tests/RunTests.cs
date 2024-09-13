using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Engine.Tests;

[TestFixture]
public class RunTests
{
    [Test]
    public void New_Default_ShouldBeExpected()
    {
        var run = new Run();

        run.RunId.Should().NotBeEmpty();
        run.Environment.Should().NotBeNull();
        run.Result.Should().Be(ResultState.None);
        run.RanBy.Should().BeEmpty();
        run.RanOn.Should().Be(default);
        run.Outcomes.Should().BeEmpty();
    }

    [Test]
    public void New_EnvironmentOverload_ShouldHaveExpectedValue()
    {
        var environment = new Environment();
        var run = new Run(environment);

        run.Environment.Should().BeEquivalentTo(environment);
    }

    [Test]
    public void New_EnvironmentAndSeed_ShouldHaveExpectedValue()
    {
        var container = Node.NewCollection();
        container.AddSpec();
        container.AddSpec();
        container.AddSpec();
        var environment = new Environment();

        var run = new Run(environment, container);

        run.Environment.Should().BeEquivalentTo(environment);
        run.Outcomes.Should().HaveCount(3);
    }

    [Test]
    public void AddNode_NullNode_ShouldThrowException()
    {
        var run = new Run();

        FluentActions.Invoking(() => run.AddNode(null!)).Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void AddNode_ValidSpecNode_ShouldHaveExpectedOutcomeCount()
    {
        var node = Node.NewSpec();
        var run = new Run();

        run.AddNode(node);

        run.Outcomes.Should().HaveCount(1);
    }

    [Test]
    public void AddNode_EmptyCollectionNode_ShouldHaveExpectedOutcomeCount()
    {
        var node = Node.NewCollection();
        var run = new Run();

        run.AddNode(node);

        run.Outcomes.Should().BeEmpty();
    }

    [Test]
    public void AddNode_ContainerWithManySpecs_ShouldHaveExpectedOutcomeCount()
    {
        var container = Node.NewCollection();
        container.AddSpec();
        container.AddSpec();
        container.AddSpec();
        var run = new Run();

        run.AddNode(container);

        run.Outcomes.Should().HaveCount(3);
    }

    [Test]
    public void AddNodes_ManySpecOutcomes_ShouldHaveExpectedOutcomeCount()
    {
        var run = new Run();

        run.AddNodes([Node.NewSpec(), Node.NewSpec(), Node.NewSpec()]);

        run.Outcomes.Should().HaveCount(3);
    }

    [Test]
    public void AddNodes_ManyDifferentOutcomes_ShouldHaveExpectedOutcomeCount()
    {
        var run = new Run();

        run.AddNodes([Node.NewSpec(), Node.NewContainer(), Node.NewCollection()]);

        run.Outcomes.Should().HaveCount(1);
    }

    [Test]
    public void Clear_WhenCalled_ShouldHaveExpectedCount()
    {
        var run = new Run();
        run.AddNodes([Node.NewSpec(), Node.NewSpec(), Node.NewSpec()]);

        run.Clear();

        run.Outcomes.Should().BeEmpty();
    }

    [Test]
    public void RemoveNode_NullNode_ShouldThrowException()
    {
        var run = new Run();

        FluentActions.Invoking(() => run.RemoveNode(null!)).Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void RemoveNode_NoOutcomes_ShouldHaveExpectedCount()
    {
        var run = new Run();

        run.RemoveNode(Node.NewSpec());

        run.Outcomes.Should().BeEmpty();
    }

    [Test]
    public void RemoveNode_ValidNode_ShouldHaveExpectedCount()
    {
        var run = new Run();
        var spec = Node.NewSpec();

        run.AddNode(spec);
        run.Outcomes.Should().HaveCount(1);

        run.RemoveNode(spec);
        run.Outcomes.Should().BeEmpty();
    }

    [Test]
    public void RemoveNodes_SomeExistSomeDont_ShouldHaveExpectedCount()
    {
        var spec = Node.NewSpec();
        var run = new Run();
        run.AddNodes([spec, Node.NewSpec(), Node.NewSpec()]);

        run.RemoveNodes([spec, Node.NewSpec(), Node.NewSpec()]);

        run.Outcomes.Should().HaveCount(2);
    }

    [Test]
    public async Task Execute_SimpleCheck_ShouldHaveExpectedResults()
    {
        var environment = new Environment();
        environment.Add(new Uri(Known.Test));

        var spec = Node.NewSpec("Test",
            s => { s.Find(Element.Module).Verify("Inhibited", Operation.False); });


        var run = new Run(environment);
        run.AddNode(spec);

        await run.Execute([spec]);

        run.Result.Should().Be(ResultState.Passed);
        run.RanBy.Should().NotBeEmpty();
        run.RanOn.Should().BeWithin(TimeSpan.FromSeconds(1));
        run.Outcomes.First().Evaluations.Should().NotBeEmpty();
    }

    [Test]
    public async Task Execute_ValidSpecValidSource_ShouldHaveExpectedResults()
    {
        var environment = new Environment();
        environment.Add(new Uri(Known.Test));

        var spec = Node.NewSpec("Test", s =>
        {
            s.Find(Element.DataType)
                .Filter("Name", Operation.EqualTo, "ComplexType")
                .Verify("Members", Operation.Any,
                    new Criterion(Element.DataTypeMember.Property("DataType"), Operation.EqualTo, "SimpleType"));
        });

        var run = new Run(environment);
        run.AddNode(spec);

        await run.Execute([spec]);

        run.Result.Should().Be(ResultState.Passed);
        run.RanBy.Should().NotBeEmpty();
        run.RanOn.Should().BeWithin(TimeSpan.FromSeconds(1));
        run.Outcomes.First().Evaluations.Should().NotBeEmpty();
    }
}