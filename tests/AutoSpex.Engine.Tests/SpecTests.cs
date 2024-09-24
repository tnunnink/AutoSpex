using System.Text.Json;
using JetBrains.dotMemoryUnit;
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
    public async Task RunAsync_ValidSpec_ShouldReturnSuccess()
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
    public async Task RunAsync_InhibitedIsFalse_ShouldReturnSuccess()
    {
        var spec = new Spec();
        var content = L5X.Load(Known.Test);

        spec.Find(Element.Module)
            .Verify("Inhibited", Negation.Is, Operation.False);

        var verification = await spec.RunAsync(content);

        verification.Result.Should().Be(ResultState.Passed);
        verification.Evaluations.Should().NotBeEmpty();
    }

    [DotMemoryUnit(FailIfRunWithoutSupport = false)]
    [Test]
    public void CheckForMemeoryLeaksAgainstSpec()
    {
        var isolator = new Action(() =>
        {
            var content = L5X.Load(Known.Test);
            var spec = Spec.Configure(c =>
            {
                c.Find(Element.Module);
                c.Verify("Inhibited", Negation.Is, Operation.False);
            });

            var verification = spec.Run(content);
            verification.Result.Should().Be(ResultState.Passed);
        });

        isolator();

        GC.Collect();
        GC.WaitForFullGCComplete();

        // Assert L5X is removed from memory
        dotMemory.Check(memory => memory.GetObjects(where => where.Type.Is<L5X>()).ObjectsCount.Should().Be(0));
        dotMemory.Check(memory => memory.GetObjects(where => where.Type.Is<Spec>()).ObjectsCount.Should().Be(0));
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