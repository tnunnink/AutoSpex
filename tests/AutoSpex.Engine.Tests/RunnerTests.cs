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
            s.Query(Element.Tag);
            s.Where("TagName", Operation.EqualTo, "TestSimpleTag");
            s.Verify("DataType", Operation.EqualTo, "SimpleType");
        });
        var run = new Run(spec, source);

        var result = await Runner.Run(run);

        result.Result.Should().Be(ResultState.Passed);
    }

    [Test]
    public async Task Run_SameRunMultipleTimes_ShouldReturnExpectedResult()
    {
        var source = Source.Create(Known.Test);
        var spec = Node.NewSpec("TestSpec", s =>
        {
            s.Query(Element.Tag);
            s.Where("TagName", Operation.EqualTo, "TestSimpleTag");
            s.Verify("DataType", Operation.EqualTo, "SimpleType");
        });
        var run = new Run(spec, source);

        var results = await Runner.Run([run, run, run]);

        results.Should().AllSatisfy(r => r.Result.Should().Be(ResultState.Passed));
    }
    
    [Test]
    [TestCase(20)]
    [TestCase(50)]
    [TestCase(100)]
    public async Task Run_VaryingNumberOfTimes_ShouldNotCompletelyFuckEverythingUp(int limit)
    {
        var source = Source.Create(Known.Test);
        var spec = Node.NewSpec("TestSpec", s =>
        {
            s.Query(Element.Tag);
            s.Where("TagName", Operation.EqualTo, "TestSimpleTag");
            s.Verify("DataType", Operation.EqualTo, "SimpleType");
        });
        var runs = Enumerable.Range(0, limit).Select(_ => new Run(spec, source)).ToArray();

        var results = await Runner.Run(runs);

        results.Should().AllSatisfy(r => r.Result.Should().Be(ResultState.Passed));
    }
}