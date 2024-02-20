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
        var spec = Node.NewSpec("MySpec");
        spec.Configure(c =>
        {
            c.Element = Element.Tag;
            c.Filters.Add(new Criterion("Name", Operation.Equal, "TestSimpleTag"));
            c.Verifications.Add(new Criterion("DataType", Operation.Equal, "SimpleType"));
        });

        var result = await spec.Run(source);

        result.Result.Should().Be(ResultState.Passed);
        result.Spec.Should().Be("MySpec");
        result.NodeId.Should().Be(spec.NodeId);
        result.Verifications.Should().HaveCount(2);
        result.ProducedOn.Should().BeWithin(TimeSpan.FromSeconds(1));
        result.Duration.Should().BeLessThan(1000);
    }
    
    [Test]
    public async Task Run_SpecManyTimes_ShouldRunRelativelyQuickly()
    {
        var source = new Source(L5X.Load(Known.Test));
        var spec = Node.NewSpec("MySpec");
        spec.Configure(c =>
        {
            c.Element = Element.Tag;
            c.Filters.Add(new Criterion("Name", Operation.Equal, "TestSimpleTag"));
            c.Verifications.Add(new Criterion("DataType", Operation.Equal, "SimpleType"));
        });

        var results = new List<Outcome>();
        var stopwatch = Stopwatch.StartNew();
        for (var i = 0; i < 100; i++)
        {
            var result = await spec.Run(source);
            results.Add(result);
            result.Result.Should().Be(ResultState.Passed);
        }
        stopwatch.Stop();
        
        Console.WriteLine(stopwatch.ElapsedMilliseconds);
        results.Should().NotBeEmpty();
        var average = results.Select(r => r.Duration).Sum() / results.Count;
        Console.WriteLine(average);
    }
    
    [Test]
    public async Task Run_5094IB16A_ShouldBeAtRevision_2_011()
    {
        var source = new Source(L5X.Load(Known.Example));
        var spec = Node.NewSpec();
        spec.Configure(c =>
        {
            c.Element = Element.Module;
            c.Where("CatalogNumber", Operation.Equal, "5094-IB16/A");
            c.Verify("Revision", Operation.Equal, "2.11");
        });

        var result = await spec.Run(source);

        result.Result.Should().Be(ResultState.Passed);
        result.Duration.Should().BeLessThan(1000);
    }

    [Test]
    public void Hashing()
    {
        var first = "Decimal";
        var second = "DECIMAL";

        Console.WriteLine(StringComparer.OrdinalIgnoreCase.GetHashCode(first));
        Console.WriteLine(StringComparer.OrdinalIgnoreCase.GetHashCode(second));
        
        Console.WriteLine(first.StableHash());
        Console.WriteLine(second.StableHash());
    }
}