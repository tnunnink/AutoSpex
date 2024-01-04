using AutoSpex.Engine.Operations;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Engine.Tests;

[TestFixture]
public class SpecificationTests
{
    [Test]
    public async Task Run_InPrinciple_ShouldRun()
    {
        var content = L5X.Load(Known.Test);
        var runner = new Runner(content);
        var spec = Specification.For(Element.Tag);

        var result = await runner.Run(spec);

        result.Should().NotBeNull();
    }

    [Test]
    public async Task Run_ValidFilterAndVerification_ShouldBeExpected()
    {
        var content = L5X.Load(Known.Test);
        var runner = new Runner(content);
        
        var spec = Specification.For(Element.Tag)
            .WithFilter(Element.Has("Name", Operation.Equal, "TestSimpleTag"))
            .Verify(Element.Has("DataType", Operation.Equal, "SimpleType"));

        var result = await runner.Run(spec);
        
        result.Result.Should().Be(ResultType.Passed);
    }

    [Test]
    public async Task Run_5094IB16A_ShouldBeAtRevision_2_011()
    {
        var content = L5X.Load(Known.Example);
        var runner = new Runner(content);
        var spec = Specification.For(Element.Module);

        spec.WithFilter(Element.Has("CatalogNumber", Operation.Equal, "5094-IB16/A"));
        spec.Verify(Element.Has("Revision", Operation.Equal, "2.11"));

        var result = await runner.Run(spec);
        
        result.Result.Should().Be(ResultType.Passed);
    }
    
    [Test]
    public async Task Run_VerifyWithNoResults_ShouldBeFailed()
    {
        var content = L5X.Load(Known.Test);
        var runner = new Runner(content);
        var spec = Specification.For(Element.Tag);

        spec.WithFilter(Element.Has("Name", Operation.Equal, "Fake"));
        spec.Verify(Element.Has("DataType", Operation.Equal, "SimpleType"));

        var result = await runner.Run(spec);
        
        result.Result.Should().Be(ResultType.Failed);
    }
    
    /*[Test]
    public async Task Run_FailedRangePassedVerification_ShouldBeFailed()
    {
        var content = L5X.Load(Known.Test);
        var spec = new Specification(Element.Tag);

        spec.ApplyRange(Operation.EqualTo, 1);
        spec.ApplyFilter(Criterion.For<Tag>("Name", Operation.Contains, "Test"));
        spec.AddVerification(Criterion.For<Tag>("AliasFor", Operation.IsNull));

        var result = await spec.Run(content);
        
        result.Result.Should().Be(ResultType.Failed);
    }

    [Test]
    public async Task Run_MultipleVerificationsOnePassOnFailWithAnyConfig_ShouldPass()
    {
        var content = L5X.Load(Known.Test);
        var spec = new Specification(Element.Tag);
        
        spec.ApplyFilter(Criterion.For<Tag>("Name", Operation.EqualTo, "TestSimpleTag"));
        spec.AddVerification(Criterion.For<Tag>("DataType", Operation.EqualTo, "SimpleType"));
        spec.AddVerification(Criterion.For<Tag>("Radix", Operation.EqualTo, "Decimal"));

        var config = new RunConfig
        {
            VerificationInclusion = InclusionType.Any
        };
        
        var result = await spec.Run(content, config);
        
        result.Result.Should().Be(ResultType.Passed);
    }*/
}