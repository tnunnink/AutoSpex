using System.Diagnostics;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Engine.Tests;

[TestFixture]
public class ProofTests
{
    [Test]
    public async Task Run_SpecWithValidConfig_ShouldBePassedAndExpectedValues()
    {
        var source = await L5X.LoadAsync(Known.Test);
        var spec = Spec.Configure(c =>
        {
            c.Get(Element.Tag);
            c.Where("TagName", Operation.EqualTo, "TestSimpleTag");
            c.Validate("DataType", Operation.EqualTo, "SimpleType");
        });

        var results = await spec.RunAsync(source);

        results.Should().AllSatisfy(e => e.Result.Should().Be(ResultState.Passed));
        results.Should().HaveCount(1);
    }

    [Test]
    public async Task Run_SpecManyTimes_ShouldRunRelativelyQuickly()
    {
        var source = await L5X.LoadAsync(Known.Test);
        var spec = Spec.Configure(c =>
        {
            c.Get(Element.Tag);
            c.Where("TagName", Operation.EqualTo, "TestSimpleTag");
            c.Validate("DataType", Operation.EqualTo, "SimpleType");
        });

        var stopwatch = Stopwatch.StartNew();
        var evaluations = new List<Evaluation>();

        for (var i = 0; i < 100; i++)
        {
            var result = await spec.RunAsync(source);
            evaluations.AddRange(result);
        }

        stopwatch.Stop();

        Console.WriteLine(stopwatch.ElapsedMilliseconds);
        evaluations.Should().NotBeEmpty();
    }

    [Test]
    public async Task Run_5094IB16A_ShouldBeAtRevision_2_011()
    {
        var source = await L5X.LoadAsync(Known.Test);

        var spec = Spec.Configure(c =>
        {
            c.Get(Element.Module);
            c.Where("CatalogNumber", Operation.EqualTo, "5094-IB16/A");
            c.Validate("Revision", Operation.EqualTo, "2.1");
        });

        var results = await spec.RunAsync(source);

        results.Should().AllSatisfy(e => e.Result.Should().Be(ResultState.Passed));
    }

    [Test]
    public void ValueExtraction()
    {
        var source = L5X.Load(Known.Example);

        var tagNames = source.Query<Tag>()
            .Select(t => new
            {
                t.DataType,
                Members = t.TagNames().Where(x => !string.IsNullOrWhiteSpace(x.Path)).Select(n => n.Path).ToList()
            })
            .DistinctBy(t => t.DataType)
            .ToList();

        tagNames.Should().NotBeEmpty();
    }
}