using System.Diagnostics;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Engine.Tests;

[TestFixture]
public class ProofTests
{
    [Test]
    public async Task Run_SpecWithValidConfig_ShouldBePassedAndExpectedValues()
    {
        var source = new Source(L5X.Load(Known.Test));
        var spec = new Spec();
        spec.Query(Element.Tag);
        spec.Where(Element.Tag.Property("TagName"), Operation.Equal, "TestSimpleTag");
        spec.Verify(Element.Tag.Property("DataType"), Operation.Equal, "SimpleType");

        var outcome = await spec.Run(source);

        outcome.Result.Should().Be(ResultState.Passed);
        outcome.Evaluations.Should().HaveCount(1);
    }

    [Test]
    public async Task Run_SpecManyTimes_ShouldRunRelativelyQuickly()
    {
        var source = new Source(L5X.Load(Known.Test));
        var spec = Spec.Configure(c =>
        {
            c.Query(Element.Tag);
            c.Where(Element.Tag.Property("TagName"), Operation.Equal, "TestSimpleTag");
            c.Verify(Element.Tag.Property("DataType"), Operation.Equal, "SimpleType");
        });

        var results = new List<Outcome>();
        var stopwatch = Stopwatch.StartNew();
        for (var i = 0; i < 100; i++)
        {
            var outcome = await spec.Run(source);
            results.Add(outcome);
        }

        stopwatch.Stop();

        Console.WriteLine(stopwatch.ElapsedMilliseconds);
        results.Should().NotBeEmpty();
    }

    [Test]
    public async Task Run_5094IB16A_ShouldBeAtRevision_2_011()
    {
        var source = new Source(L5X.Load(Known.Example));
        var spec = Spec.Configure(c =>
        {
            c.Query(Element.Module);
            c.Where(Element.Module.Property("CatalogNumber"), Operation.Equal, "5094-IB16/A");
            c.Verify(Element.Module.Property("Revision"), Operation.Equal, "2.11");
        });

        var outcome = await spec.Run(source);

        outcome.Result.Should().Be(ResultState.Passed);
    }
}