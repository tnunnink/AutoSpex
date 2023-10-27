using L5Sharp;
using L5Sharp.Common;
using L5Spex.Engine.Enumerations;
using L5Spex.Engine.Operations;
using Task = System.Threading.Tasks.Task;

namespace L5Spex.Engine.Tests;

[TestFixture]
public class SpecificationTests
{
    [Test]
    public async Task Run_InPrinciple_ShouldRun()
    {
        var content = LogixContent.Load(Known.Test);
        var spec = new Specification<Tag>();

        var result = await spec.Run(content);

        result.Should().NotBeNull();
    }

    [Test]
    public async Task Run_ValidFilterAndVerification_ShouldBeExpected()
    {
        var content = LogixContent.Load(Known.Test);
        var spec = new Specification<Tag>();

        spec.ApplyFilter(Criterion.For<Tag>("Name", Operation.EqualTo, "TestSimpleTag"));
        spec.AddVerification(Criterion.For<Tag>("DataType", Operation.EqualTo, "SimpleType"));

        var result = await spec.Run(content);
        
        result.Result.Should().Be(ResultType.Passed);
    }

    [Test]
    public async Task Run_5094IB16A_ShouldBeAtRevision_2_011()
    {
        var content = LogixContent.Load(Known.Example);
        var spec = new Specification<Module>();

        spec.ApplyFilter(Criterion.For<Module>("CatalogNumber", Operation.EqualTo, "5094-IB16/A"));
        spec.AddVerification(Criterion.For<Module>("Revision", Operation.EqualTo, new Revision("2.11")));

        var result = await spec.Run(content);
        
        result.Result.Should().Be(ResultType.Passed);
    }
    
    [Test]
    public async Task Run_VerifyWithNoResults_ShouldBeFailed()
    {
        var content = LogixContent.Load(Known.Test);
        var spec = new Specification<Tag>();

        spec.ApplyFilter(Criterion.For<Tag>("Name", Operation.EqualTo, "Fake"));
        spec.AddVerification(Criterion.For<Tag>("DataType", Operation.EqualTo, "SimpleType"));

        var result = await spec.Run(content);
        
        result.Result.Should().Be(ResultType.Failed);
    }
    
    [Test]
    public async Task Run_FailedRangePassedVerification_ShouldBeFailed()
    {
        var content = LogixContent.Load(Known.Test);
        var spec = new Specification<Tag>();

        spec.ApplyRange(Operation.EqualTo, 1);
        spec.ApplyFilter(Criterion.For<Tag>("Name", Operation.Contains, "Test"));
        spec.AddVerification(Criterion.For<Tag>("AliasFor", Operation.IsNull));

        var result = await spec.Run(content);
        
        result.Result.Should().Be(ResultType.Failed);
    }

    [Test]
    public async Task Run_MultipleVerificationsOnePassOnFailWithAnyConfig_ShouldPass()
    {
        var content = LogixContent.Load(Known.Test);
        var spec = new Specification<Tag>();
        
        spec.ApplyFilter(Criterion.For<Tag>("Name", Operation.EqualTo, "TestSimpleTag"));
        spec.AddVerification(Criterion.For<Tag>("DataType", Operation.EqualTo, "SimpleType"));
        spec.AddVerification(Criterion.For<Tag>("Radix", Operation.EqualTo, "Decimal"));

        var config = new RunConfig
        {
            VerificationInclusion = InclusionType.Any
        };
        
        var result = await spec.Run(content, config);
        
        result.Result.Should().Be(ResultType.Passed);
    }
}