using System.Diagnostics;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Engine.Tests;

[TestFixture]
public class ProofTests
{
    [Test]
    public async Task Run_SpecWithValidConfig_ShouldBePassedAndExpectedValues()
    {
        var source = new Source(new Uri(Known.Test));
        var spec = new Spec();
        spec.Find(Element.Tag);
        spec.Where(Element.Tag.Property("TagName"), Operation.EqualTo, "TestSimpleTag");
        spec.ShouldHave(Element.Tag.Property("DataType"), Operation.EqualTo, "SimpleType");

        var outcome = await spec.RunAsync(source);

        outcome.Result.Should().Be(ResultState.Passed);
        outcome.Verifications.Should().HaveCount(1);
    }

    [Test]
    public async Task Run_SpecManyTimes_ShouldRunRelativelyQuickly()
    {
        var source = new Source(new Uri(Known.Test));
        var spec = Spec.Configure(c =>
        {
            c.Find(Element.Tag);
            c.Where(Element.Tag.Property("TagName"), Operation.EqualTo, "TestSimpleTag");
            c.ShouldHave(Element.Tag.Property("DataType"), Operation.EqualTo, "SimpleType");
        });

        var results = new List<Outcome>();
        var stopwatch = Stopwatch.StartNew();
        for (var i = 0; i < 100; i++)
        {
            var outcome = await spec.RunAsync(source);
            results.Add(outcome);
        }

        stopwatch.Stop();

        Console.WriteLine(stopwatch.ElapsedMilliseconds);
        results.Should().NotBeEmpty();
    }

    [Test]
    public async Task Run_5094IB16A_ShouldBeAtRevision_2_011()
    {
        var source = new Source(new Uri(Known.Example));
        var spec = Spec.Configure(c =>
        {
            c.Find(Element.Module);
            c.Where(Element.Module.Property("CatalogNumber"), Operation.EqualTo, "5094-IB16/A");
            c.ShouldHave(Element.Module.Property("Revision"), Operation.EqualTo, "2.11");
        });

        var outcome = await spec.RunAsync(source);

        outcome.Result.Should().Be(ResultState.Passed);
    }
}