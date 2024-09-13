using System.Text.Json;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Engine.Tests;

[TestFixture]
public class SpecTests
{
    [Test]
    public void New_Default_ShouldHaveExpectedValues()
    {
        var spec = new Spec();

        spec.Query.Should().NotBeNull();
        spec.Filters.Should().BeEmpty();
        spec.Verifications.Should().BeEmpty();
        spec.FilterInclusion.Should().Be(Inclusion.All);
        spec.VerificationInclusion.Should().Be(Inclusion.All);
    }

    [Test]
    public void FluentBuild_WhenCalled_ShouldUpdateAsExpected()
    {
        var spec = new Spec();

        spec.Find(Element.Tag)
            .Filter("Name", Operation.Containing, "Test")
            .Verify("Value", Operation.EqualTo, 4);

        spec.Query.Element.Should().Be(Element.Tag);
        spec.Filters.Should().HaveCount(1);
        spec.Verifications.Should().HaveCount(1);
    }

    [Test]
    public async Task RunAsync_DefaultElement_ShouldReturnNoneDueToNoVerifications()
    {
        var spec = new Spec();
        var content = L5X.Load(Known.Test);

        var verification = await spec.RunAsync(content);

        verification.Result.Should().Be(ResultState.Inconclusive);
        verification.Evaluations.Should().BeEmpty();
    }

    [Test]
    public async Task RunAsync_DefaultElement_ShouldReturnNoneDueToNoVerifications_ValidSpec_ShouldReturnSuccess()
    {
        var spec = new Spec();
        var content = L5X.Load(Known.Test);

        spec.Find(Element.Tag)
            .Filter("Name", Operation.Containing, "Test")
            .Verify("DataType", Negation.Not, Operation.NullOrEmpty);

        var verification = await spec.RunAsync(content);

        verification.Result.Should().Be(ResultState.Passed);
        verification.Evaluations.Should().NotBeEmpty();
    }

    [Test]
    public async Task RunAsync_WithRangeValidRange_ShouldReturnSuccess()
    {
        var spec = new Spec();
        var content = L5X.Load(Known.Test);

        spec.Find(Element.Program)
            .Filter("Type", Operation.EqualTo, ProgramType.Normal)
            .Count(Operation.GreaterThanOrEqualTo, 1);

        var verification = await spec.RunAsync(content);

        verification.Result.Should().Be(ResultState.Passed);
    }

    [Test]
    public async Task RunAsync_WithRangeInvalidRange_ShouldReturnFailure()
    {
        var spec = new Spec();
        var content = L5X.Load(Known.Test);

        spec.Find(Element.Program)
            .Filter("Type", Operation.EqualTo, ProgramType.Normal)
            .Count(Operation.LessThan, 1);

        var verification = await spec.RunAsync(content);

        verification.Result.Should().Be(ResultState.Failed);
    }

    [Test]
    public Task Serialize_ConfiguredSpec_ShouldBeVerified()
    {
        var spec = new Spec();

        spec.Find(Element.Tag)
            .Filter("Name", Operation.Containing, "Test")
            .Verify("DataType", Negation.Not, Operation.NullOrEmpty);

        return VerifyJson(JsonSerializer.Serialize(spec));
    }

    [Test]
    public void Deserialize_WhenCalled_ShouldBeExpected()
    {
        var spec = new Spec();
        spec.Find(Element.Tag)
            .Filter("Name", Operation.Containing, "Test")
            .Verify("DataType", Negation.Not, Operation.NullOrEmpty);
        var data = JsonSerializer.Serialize(spec);

        var result = JsonSerializer.Deserialize<Spec>(data);

        result?.Query.Element.Should().Be(Element.Tag);
        result?.FilterInclusion.Should().Be(Inclusion.All);
        result?.VerificationInclusion.Should().Be(Inclusion.All);
        result?.Filters.Should().HaveCount(1);
        result?.Verifications.Should().HaveCount(1);

        result.Should().BeEquivalentTo(spec);
    }
}