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
        spec.Settings.VerifyCount.Should().BeFalse();
        spec.Settings.CountOperation.Should().Be(Operation.GreaterThan);
        spec.Settings.CountValue.Should().Be(0);
        spec.Settings.FilterInclusion.Should().Be(Inclusion.All);
        spec.Settings.VerificationInclusion.Should().Be(Inclusion.All);
    }

    [Test]
    public void FluentBuild_WhenCalled_ShouldUpdateAsExpected()
    {
        var spec = new Spec();

        spec.Query(Element.Module);
        spec.Where(Element.Tag.Property("Name"), Operation.Contains, "Test");
        spec.Verify(Element.Tag.Property("Value"), Operation.In, 1, 2, 3, 4, 5);

        spec.Element.Should().Be(Element.Module);
        spec.Filters.Should().HaveCount(1);
        spec.Verifications.Should().HaveCount(1);
    }

    [Test]
    public async Task Run_DefaultElement_ShouldReturnNoneDueToNoVerifications()
    {
        var spec = new Spec();
        var source = new Source(L5X.Load(Known.Test));

        var outcome = await spec.Run(source);

        outcome.Result.Should().Be(ResultState.None);
    }

    [Test]
    public async Task Run_DefaultElementWithVerifyCountSetFalse_ShouldReturnNoEvaluations()
    {
        var source = new Source(L5X.Load(Known.Test));
        var spec = new Spec
        {
            Settings =
            {
                VerifyCount = false
            }
        };

        var outcome = await spec.Run(source);

        outcome.Evaluations.Should().BeEmpty();
    }
}