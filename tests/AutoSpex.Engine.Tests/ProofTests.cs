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
        var spec = Spec.Configure(c =>
        {
            c.Query(Element.Tag);
            c.Filter("TagName", Operation.EqualTo, "TestSimpleTag");
            c.Verify("DataType", Operation.EqualTo, "SimpleType");
        });


        var outcome = await spec.RunAsync(source);

        outcome.Result.Should().Be(ResultState.Passed);
        outcome.Evaluations.Should().HaveCount(1);
    }

    [Test]
    public async Task Run_SpecManyTimes_ShouldRunRelativelyQuickly()
    {
        var source = L5X.Load(Known.Test);
        var spec = Spec.Configure(c =>
        {
            c.Query(Element.Tag);
            c.Filter("TagName", Operation.EqualTo, "TestSimpleTag");
            c.Verify("DataType", Operation.EqualTo, "SimpleType");
        });

        var stopwatch = Stopwatch.StartNew();
        var verification = Verification.None;

        for (var i = 0; i < 100; i++)
        {
            var result = await spec.RunAsync(source);
            verification = Verification.Merge([verification, result]);
        }

        stopwatch.Stop();

        Console.WriteLine(stopwatch.ElapsedMilliseconds);
        verification.Evaluations.Should().NotBeEmpty();
    }

    [Test]
    public async Task Run_5094IB16A_ShouldBeAtRevision_2_011()
    {
        var source = L5X.Load(Known.Test);

        var spec = Spec.Configure(c =>
        {
            c.Query(Element.Module);
            c.Filter("CatalogNumber", Operation.EqualTo, "5094-IB16/A");
            c.Verify("Revision", Operation.EqualTo, "2.1");
        });

        var verification = await spec.RunAsync(source);

        verification.Result.Should().Be(ResultState.Passed);
    }
}