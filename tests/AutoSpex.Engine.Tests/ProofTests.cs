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

        var verifications = (await spec.Run(source)).ToList();

        verifications.Should().AllSatisfy(v => v.Result.Should().Be(ResultState.Passed));
        verifications.SelectMany(v => v.Evaluations).Should().HaveCount(2);
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

        var results = new List<Verification>();
        var stopwatch = Stopwatch.StartNew();
        for (var i = 0; i < 100; i++)
        {
            var verifications = await spec.Run(source);
            results.AddRange(verifications);
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

        var verifications = (await spec.Run(source)).ToList();

        verifications.Max(r => r.Result).Should().Be(ResultState.Passed);
    }
}