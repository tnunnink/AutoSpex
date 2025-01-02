using System.Text.Json;
using JetBrains.dotMemoryUnit;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Engine.Tests;

[TestFixture]
public class SpecTests
{
    private static VerifySettings VerifySettings
    {
        get
        {
            var settings = new VerifySettings();
            settings.ScrubInlineGuids();
            return settings;
        }
    }

    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    [Test]
    public void New_Default_ShouldHaveExpectedValues()
    {
        var spec = new Spec();

        spec.SpecId.Should().NotBeEmpty();
        spec.Query.Should().NotBeNull();
        spec.Verify.Should().NotBeNull();
    }

    [Test]
    public void New_ValidElement_ShouldHaveExpectedElementForQueryStep()
    {
        var spec = new Spec(Element.Tag);

        spec.Query.Element.Should().Be(Element.Tag);
    }

    [Test]
    public void FluentBuild_WhenCalled_ShouldUpdateAsExpected()
    {
        var spec = Spec.Configure(s =>
        {
            s.Get(Element.Tag);
            s.Where("Name", Operation.Containing, "Test");
            s.Select("Value");
            s.Validate("This", Operation.EqualTo, 4);
        });

        spec.Query.Element.Should().Be(Element.Tag);
        spec.Query.Steps.Should().HaveCount(2);
        spec.Verify.Criteria.Should().HaveCount(1);
    }

    [Test]
    public void Duplicate_WhenCalled_ShouldBeNewInstanceBuEquivalent()
    {
        var spec = Spec.Configure(s =>
        {
            s.Get(Element.Tag);
            s.Where("Name", Operation.Containing, "Test");
            s.Validate("Value", Operation.EqualTo, 4);
        });

        var duplicate = spec.Duplicate();

        duplicate.Should().NotBeSameAs(spec);
        duplicate.Query.Should().BeEquivalentTo(spec.Query);
        duplicate.Verify.Should().BeEquivalentTo(spec.Verify);
    }

    [Test]
    public void GetCriteria_WhenCalled_ShouldBeExpected()
    {
        var spec = Spec.Configure(s =>
        {
            s.Get(Element.Tag);
            s.Where("Name", Operation.Containing, "Test");
            s.Validate("Value", Operation.EqualTo, 4);
        });

        var criteria = spec.GetAllCriteria();

        criteria.Should().HaveCount(2);
    }

    [Test]
    public async Task RunAsync_Default_ShouldReturnNoneDueToNoVerifications()
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

        spec.Get(Element.Tag)
            .Where("Name", Operation.Containing, "Test")
            .Validate("DataType", Negation.Not, Operation.Void);

        var verification = await spec.RunAsync(content);

        verification.Result.Should().Be(ResultState.Passed);
        verification.Evaluations.Should().NotBeEmpty();
    }

    [Test]
    public async Task RunAsync_InhibitedIsFalse_ShouldReturnSuccess()
    {
        var spec = new Spec();
        var content = L5X.Load(Known.Test);
        spec.Get(Element.Module).Validate("Inhibited", Operation.EqualTo, false);

        var verification = await spec.RunAsync(content);

        verification.Result.Should().Be(ResultState.Passed);
        verification.Evaluations.Should().NotBeEmpty();
    }

    [Test]
    public Task Serialize_Default_ShouldBeVerified()
    {
        var spec = new Spec();

        var json = JsonSerializer.Serialize(spec, Options);

        return Verify(json, VerifySettings);
    }

    [Test]
    public Task Serialize_ConfiguredSpec_ShouldBeVerified()
    {
        var spec = Spec.Configure(s =>
        {
            s.Get(Element.Tag);
            s.Where("Name", Operation.Containing, "Test");
            s.Validate("DataType", Negation.Not, Operation.Void);
        });

        var json = JsonSerializer.Serialize(spec, Options);

        return Verify(json, VerifySettings);
    }

    [Test]
    public Task Serialize_ConfiguredSpecWithSelect_ShouldBeVerified()
    {
        var spec = Spec.Configure(s =>
        {
            s.Get(Element.Tag);
            s.Where("Name", Operation.Containing, "Test");
            s.Select("Members");
            s.Validate("DataType", Negation.Not, Operation.Void);
        });

        var json = JsonSerializer.Serialize(spec, Options);

        return Verify(json, VerifySettings);
    }

    [Test]
    public Task Serialize_ConfiguredSpecWithRange_ShouldBeVerified()
    {
        var spec = Spec.Configure(s =>
        {
            s.Get(Element.Tag);
            s.Where("Name", Operation.Containing, "Test");
            s.Validate("Value", Negation.Is, Operation.Between, new Range(1, 10));
        });

        var json = JsonSerializer.Serialize(spec, Options);

        return Verify(json, VerifySettings);
    }

    [Test]
    public void Deserialize_WhenCalled_ShouldBeExpected()
    {
        var spec = Spec.Configure(s =>
        {
            s.Get(Element.Tag);
            s.Where("Name", Operation.Containing, "Test");
            s.Validate("DataType", Negation.Not, Operation.Void);
        });
        var data = JsonSerializer.Serialize(spec);

        var result = JsonSerializer.Deserialize<Spec>(data);

        result.Should().BeEquivalentTo(spec);
    }

    [Test]
    public void Deserialize_SpecWithRange_ShouldBeExpected()
    {
        var spec = Spec.Configure(s =>
        {
            s.Get(Element.Tag);
            s.Where("Name", Operation.Containing, "Test");
            s.Validate("Value", Negation.Is, Operation.Between, new Range(1, 10));
        });
        var data = JsonSerializer.Serialize(spec);

        var result = JsonSerializer.Deserialize<Spec>(data);

        result.Should().BeEquivalentTo(spec);
    }

    [DotMemoryUnit(FailIfRunWithoutSupport = false)]
    [Test]
    public void CheckForMemeoryLeaksAgainstSpec()
    {
        var isolator = new System.Action(() =>
        {
            var content = L5X.Load(Known.Test);
            var spec = Spec.Configure(c =>
            {
                c.Get(Element.Module);
                c.Validate("Inhibited", Negation.Is, Operation.EqualTo, false);
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
}