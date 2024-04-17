using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Engine.Tests;

[TestFixture]
public class SpecTests
{
    [Test]
    public void New_Default_ShouldHaveExpectedValues()
    {
        var spec = new Spec();

        spec.Element.Should().Be(Element.Default);
        spec.Filters.Should().BeEmpty();
        spec.Verifications.Should().BeEmpty();
        spec.Settings.Should().NotBeNull();
        spec.Settings.VerifyCount.Should().BeTrue();
        spec.Settings.CountOperation.Should().Be(Operation.GreaterThan);
        spec.Settings.CountValue.Should().Be(0);
        spec.Settings.FilterInclusion.Should().Be(Inclusion.All);
        spec.Settings.VerificationInclusion.Should().Be(Inclusion.All);
    }

    [Test]
    public void New_Override_ShouldHaveExpectedValues()
    {
        var spec = new Spec
        {
            Element = Element.Controller,
            Settings = new SpecSettings
            {
                VerifyCount = false,
                CountOperation = Operation.LessThan,
                CountValue = 10,
                FilterInclusion = Inclusion.Any,
                VerificationInclusion = Inclusion.Any
            },
            Filters =
            {
                new Criterion("Property1", Operation.Equal, new Argument("Value1"))
            },
            Verifications =
            {
                new Criterion("Property3", Operation.LessThan, new Argument("Value3")),
                new Criterion("Property4", Operation.IsNull)
            }
        };

        spec.Element = Element.Tag;

        spec.Element.Should().Be(Element.Tag);
    }

    [Test]
    public void FluentBuild_WhenCalled_ShouldUpdateAsExpected()
    {
        var spec = new Spec();
        
        spec.Query(Element.Module);
        spec.Where("Property", Operation.Any, new Criterion("Test", Operation.Contains, 123));
        spec.Verify("Value", Operation.In, 1, 2, 3, 4, 5);

        spec.Element.Should().Be(Element.Module);
        spec.Filters.Should().HaveCount(1);
        spec.Verifications.Should().HaveCount(1);
    }

    [Test]
    public async Task Run_DefaultElement_ShouldReturnFailedDueToFailedCountVerification()
    {
        var spec = new Spec();
        var file = L5X.Load(Known.Test);

        var outcome = await spec.Run(file);

        outcome.Result.Should().Be(ResultState.Failed);
    }

    [Test]
    public async Task Run_DefaultElementWithVerifyCountSetFalse_ShouldReturnPassed()
    {
        var content = L5X.Load(Known.Test);
        var spec = new Spec
        {
            Settings =
            {
                VerifyCount = false
            }
        };

        var outcome = await spec.Run(content);
        
        outcome.Result.Should().Be(ResultState.Passed);
        outcome.Verifications.Should().BeEmpty();
    }
}