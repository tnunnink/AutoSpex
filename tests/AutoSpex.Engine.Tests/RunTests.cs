using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Engine.Tests;

[TestFixture]
public class RunTests
{
    [Test]
    public void New_ValidNodeAndSource_ShouldHaveExpectedValues()
    {
        var source = Source.Create(Known.Test);
        var spec = Node.NewSpec("TestSpec");

        var run = new Run(spec, source);

        run.Node.Should().BeSameAs(spec);
        run.Source.Should().BeSameAs(source);
        run.State.Should().Be(ResultState.None);
        run.Progress.Should().Be(0);
        run.RanOn.Should().Be(default);
        run.RanBy.Should().BeEmpty();
        run.Duration.Should().Be(0);
    }

    [Test]
    public async Task RunAll_ValidNodeSpec_ShouldHaveExpectedValues()
    {
        var source = Source.Create(Known.Test);
        var spec = Node.NewSpec("TestSpec", s =>
        {
            s.Get(Element.Tag);
            s.Where("TagName", Operation.EqualTo, "TestSimpleTag");
            s.Validate("DataType", Operation.EqualTo, "SimpleType");
        });
        var run = new Run(spec, source);

        await run.RunAll();

        run.State.Should().Be(ResultState.Passed);
        run.Progress.Should().Be(100);
        run.RanOn.Should().BeWithin(TimeSpan.FromSeconds(1));
        run.RanBy.Should().NotBeEmpty();
        run.Duration.Should().BeGreaterThan(0);

        Directory.Delete(Repo.Cache.Location, true);
    }
}