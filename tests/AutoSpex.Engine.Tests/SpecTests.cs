using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Engine.Tests;

[TestFixture]
public class SpecTests
{
    [Test]
    public void New_Default_ShouldHaveExpectedValues()
    {
        var spec = new Spec();

        spec.SpecId.Should().NotBeEmpty();
        spec.Name.Should().NotBeEmpty();
        spec.Query.Should().NotBeNull();
        spec.Filters.Should().BeEmpty();
        spec.Verifications.Should().BeEmpty();
        spec.Range.Should().NotBeNull();
        spec.FilterInclusion.Should().Be(Inclusion.All);
        spec.VerificationInclusion.Should().Be(Inclusion.All);
    }

    [Test]
    public void FluentBuild_WhenCalled_ShouldUpdateAsExpected()
    {
        var spec = new Spec();

        spec.Find(Element.Module)
            .Where(Element.Tag.Property("Name"), Operation.Containing, "Test")
            .ShouldHave(Element.Tag.Property("Value"), Operation.In, 1, 2, 3, 4, 5);

        spec.Query.Element.Should().Be(Element.Module);
        spec.Filters.Should().HaveCount(1);
        spec.Verifications.Should().HaveCount(1);
    }

    [Test]
    public async Task Run_DefaultElement_ShouldReturnNoneDueToNoVerifications()
    {
        var spec = new Spec();
        var source = new Source(new Uri(Known.Test));

        var outcome = await spec.RunAsync(source);

        outcome.Result.Should().Be(ResultState.Inconclusive);
        outcome.Verifications.Should().BeEmpty();
    }

    [Test]
    public async Task Run_ValidSpec_ShouldReturnSuccess()
    {
        var spec = new Spec();
        var source = new Source(new Uri(Known.Test));

        spec.Find(Element.Tag)
            .Where(Element.Tag.Property("Name"), Operation.Containing, "Test")
            .ShouldNotHave(Element.Tag.Property("DataType"), Operation.NullOrEmpty);

        var outcome = await spec.RunAsync(source);

        outcome.Result.Should().Be(ResultState.Passed);
        outcome.Verifications.Should().NotBeEmpty();
    }

    [Test]
    public async Task Run_WithRangeValidRange_ShouldReturnSuccess()
    {
        var spec = new Spec();
        var source = new Source(new Uri(Known.Test));

        spec.Find(Element.Program)
            .Where(Element.Program.Property("Type"), Operation.EqualTo, ProgramType.Normal)
            .ShouldReturn(Operation.GreaterThanOrEqualTo, 1);

        var outcome = await spec.RunAsync(source);

        outcome.Result.Should().Be(ResultState.Passed);
    }

    [Test]
    public async Task Run_WithRangeInvalidRange_ShouldReturnFailure()
    {
        var spec = new Spec();
        var source = new Source(new Uri(Known.Test));

        spec.Find(Element.Program)
            .Where(Element.Program.Property("Type"), Operation.EqualTo, ProgramType.Normal)
            .ShouldReturn(Operation.LessThan, 1);

        var outcome = await spec.RunAsync(source);

        outcome.Result.Should().Be(ResultState.Failed);
    }

    [Test]
    public async Task RunAll_ValidSpec_ShouldReturnPassed()
    {
        var spec = new Spec();
        var test = new Source(new Uri(Known.Test));
        var example = new Source(new Uri(Known.Example));

        spec.Find(Element.Program)
            .Where(Element.Program.Property("Type"), Operation.EqualTo, ProgramType.Normal)
            .ShouldHave(Element.Program.Property("Disabled"), Operation.False)
            .ShouldReturn(Operation.GreaterThan, 1);

        var outcome = await spec.RunAllAsync([test, example]);

        outcome.Result.Should().Be(ResultState.Passed);
    }

    [Test]
    public Task Serialize_ConfiguredSpec_ShouldBeVerified()
    {
        var spec = new Spec();

        spec.Find(Element.Tag)
            .Where(Element.Tag.Property("Name"), Operation.Containing, "Test")
            .ShouldNotHave(Element.Tag.Property("DataType"), Operation.NullOrEmpty);

        return VerifyJson(spec.Serialize());
    }

    [Test]
    public void Deserialize_WhenCalled_ShouldBeExpected()
    {
        var spec = new Spec();
        spec.Find(Element.Tag)
            .Where(Element.Tag.Property("Name"), Operation.Containing, "Test")
            .ShouldNotHave(Element.Tag.Property("DataType"), Operation.NullOrEmpty);
        var data = spec.Serialize();

        var result = Spec.Deserialize(data);

        result?.Query.Element.Should().Be(Element.Tag);
        result?.FilterInclusion.Should().Be(Inclusion.All);
        result?.VerificationInclusion.Should().Be(Inclusion.All);
        result?.Filters.Should().HaveCount(1);
        result?.Verifications.Should().HaveCount(1);

        result.Should().BeEquivalentTo(spec, options =>
            options
                .Excluding(s => s.SpecId)
                .Excluding(s => s.Name)
                .Excluding(s => s.Node)
        );
    }
}