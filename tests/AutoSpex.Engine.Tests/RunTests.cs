using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Engine.Tests;

[TestFixture]
public class RunTests
{
    [Test]
    public void New_Empty_ShouldBeExpected()
    {
        var run = Run.Empty;

        run.RunId.Should().NotBeEmpty();
        run.Source.Should().BeEquivalentTo(Source.Empty, o => o.Excluding(s => s.Content));
        run.Node.Should().BeEquivalentTo(Node.Empty);
        run.Result.Should().Be(ResultState.None);
        run.RanBy.Should().BeEmpty();
        run.RanOn.Should().Be(default);
        run.Outcomes.Should().BeEmpty();
    }

    [Test]
    public void New_SourceAndNodeOverload_ShouldHaveEquivalentInstances()
    {
        var source = new Source();
        var node = Node.NewCollection();
        var run = new Run(node, source);

        run.RunId.Should().NotBeEmpty();
        run.Source.Should().BeEquivalentTo(source, o => o.Excluding(s => s.Content));
        run.Node.Should().BeEquivalentTo(node);
        run.Result.Should().Be(ResultState.None);
        run.RanBy.Should().BeEmpty();
        run.RanOn.Should().Be(default);
        run.Outcomes.Should().BeEmpty();
    }

    [Test]
    public void New_SourceAndNodeOverload_ShouldHaveExpectedOutcomes()
    {
        var container = Node.NewCollection();
        container.AddSpec();
        container.AddSpec();
        container.AddSpec();
        var source = new Source();

        var run = new Run(container, source);

        run.Outcomes.Should().HaveCount(3);
    }

    [Test]
    public async Task Execute_SimpleCheck_ShouldHaveExpectedResults()
    {
        var source = new Source(L5X.Load(Known.Test));

        var spec = Node.NewSpec("Test",
            s => { s.Get(Element.Module).Validate("Inhibited", Operation.EqualTo, false); });

        var run = new Run(spec, source);

        await run.Execute([spec], source);

        run.Result.Should().Be(ResultState.Passed);
        run.RanBy.Should().NotBeEmpty();
        run.RanOn.Should().BeWithin(TimeSpan.FromSeconds(1));
        run.Outcomes.First().Verification.Evaluations.Should().NotBeEmpty();
    }

    [Test]
    public async Task Execute_ValidSpecValidSource_ShouldHaveExpectedResults()
    {
        var source = new Source(L5X.Load(Known.Test));

        var spec = Node.NewSpec("Test", s =>
        {
            s.Get(Element.DataType);
            s.Where("Name", Operation.EqualTo, "ComplexType");
            s.Validate("Members", Operation.Any, new Criterion("DataType", Operation.EqualTo, "SimpleType"));
        });

        var run = new Run(spec, source);

        await run.Execute([spec], source);

        run.Result.Should().Be(ResultState.Passed);
        run.RanBy.Should().NotBeEmpty();
        run.RanOn.Should().BeWithin(TimeSpan.FromSeconds(1));
        run.Outcomes.First().Verification.Evaluations.Should().NotBeEmpty();
    }
}