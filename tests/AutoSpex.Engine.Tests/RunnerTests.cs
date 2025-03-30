using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Engine.Tests;

[TestFixture]
public class RunnerTests
{
    [Test]
    public async Task Run_SingleRun_ShouldReturnExpectedResult()
    {
        var source = Source.Create(Known.Test);
        var spec = Node.NewSpec("TestSpec", s =>
        {
            s.Get(Element.Tag);
            s.Where("TagName", Operation.EqualTo, "TestSimpleTag");
            s.Validate("DataType", Operation.EqualTo, "SimpleType");
        });
        var run = new Run(spec, source);

        var result = await Runner.Run(run);

        result.Should().Be(ResultState.Passed);
    }

    [Test]
    public async Task Run_SameRunMultipleTimes_ShouldReturnExpectedResult()
    {
        var source = Source.Create(Known.Test);
        var spec = Node.NewSpec("TestSpec", s =>
        {
            s.Get(Element.Tag);
            s.Where("TagName", Operation.EqualTo, "TestSimpleTag");
            s.Validate("DataType", Operation.EqualTo, "SimpleType");
        });
        var run = new Run(spec, source);

        var result = await Runner.Run([run, run, run]);

        result.Should().Be(ResultState.Passed);
    }
}