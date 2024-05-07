using System.Diagnostics;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Engine.Tests;

[TestFixture]
public class ProofTests
{
    [Test]
    public async Task Run_SpecWithValidConfig_ShouldBePassedAndExpectedValues()
    {
        var source = L5X.Load(Known.Test);
        var spec = new Spec();
        spec.Query(Element.Tag);
        spec.Where("TagName", Operation.Equal, "TestSimpleTag");
        spec.Verify("DataType", Operation.Equal, "SimpleType");

        var outcome = await spec.Run(source);

        outcome.Result.Should().Be(ResultState.Passed);
        outcome.Evaluations.Should().HaveCount(2);
    }

    [Test]
    public async Task Run_SpecManyTimes_ShouldRunRelativelyQuickly()
    {
        var source = L5X.Load(Known.Test);
        var spec = Spec.Configure(c =>
        {
            c.Query(Element.Tag);
            c.Where("TagName", Operation.Equal, "TestSimpleTag");
            c.Verify("DataType", Operation.Equal, "SimpleType");
        });

        var results = new List<Outcome>();
        var stopwatch = Stopwatch.StartNew();
        for (var i = 0; i < 100; i++)
        {
            var result = await spec.Run(source);
            results.Add(result);
        }

        stopwatch.Stop();

        Console.WriteLine(stopwatch.ElapsedMilliseconds);
        results.Should().NotBeEmpty();
    }

    [Test]
    public async Task Run_5094IB16A_ShouldBeAtRevision_2_011()
    {
        var source = L5X.Load(Known.Example);
        var spec = Spec.Configure(c =>
        {
            c.Query(Element.Module);
            c.Where("CatalogNumber", Operation.Equal, "5094-IB16/A");
            c.Verify("Revision", Operation.Equal, "2.11");
        });

        var outcome = await spec.Run(source);

        outcome.Result.Should().Be(ResultState.Passed);
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

    [Test]
    public void ValueBrushTesting()
    {
        double number = 1.23;
        bool flag = true;
        string text = "this is a test";
        Radix option = Radix.Ascii;
        Tag element = new Tag();
        var member = element.Members();
        
        Assert.Pass();
    }
}