using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Engine.Tests;

[TestFixture]
public class ProofTests
{
    [Test]
    public async Task Run_InPrinciple_ShouldRun()
    {
        var content = L5X.Load(Known.Test);
        var runner = new SpecRunner(content);
        var spec = Specification.For(Element.Tag);

        var result = await runner.Run(spec);

        result.Should().NotBeNull();
    }

    [Test]
    public async Task Run_ValidFilterAndVerification_ShouldBeExpected()
    {
        var content = L5X.Load(Known.Test);
        var runner = new SpecRunner(content);

        var spec = Specification.For(Element.Tag);
        spec.Filter(new Criterion("Name", Operation.Equal, "TestSimpleTag"));
        spec.Verify(new Criterion("DataType", Operation.Equal, "SimpleType"));

        var result = await runner.Run(spec);

        result.Result.Should().Be(ResultType.Passed);
    }

    [Test]
    public async Task Run_5094IB16A_ShouldBeAtRevision_2_011()
    {
        var content = L5X.Load(Known.Example);
        var runner = new SpecRunner(content);
        var spec = Specification.For(Element.Module);

        spec.Filter(new Criterion("CatalogNumber", Operation.Equal, "5094-IB16/A"));
        spec.Verify(new Criterion("Revision", Operation.Equal, "2.11"));

        var result = await runner.Run(spec);

        result.Result.Should().Be(ResultType.Passed);
    }

    [Test]
    public async Task Run_VerifyWithNoResults_ShouldBeFailed()
    {
        var content = L5X.Load(Known.Test);
        var runner = new SpecRunner(content);
        var spec = Specification.For(Element.Tag);

        spec.Filter(new Criterion("Name", Operation.Equal, "Fake"));
        spec.Verify(new Criterion("DataType", Operation.Equal, "SimpleType"));

        var result = await runner.Run(spec);

        result.Result.Should().Be(ResultType.Failed);
    }
}