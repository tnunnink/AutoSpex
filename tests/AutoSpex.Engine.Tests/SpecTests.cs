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
    public void New_Default_ShouldBeExpected()
    {
        var query = new Spec();

        query.Element.Should().Be(Element.Default);
        query.Steps.Should().BeEmpty();
    }

    [Test]
    public void New_Element_ShouldBeExpected()
    {
        var query = new Spec(Element.Task);

        query.Element.Should().Be(Element.Task);
        query.Steps.Should().BeEmpty();
        query.Returns.Should().BeEquivalentTo(Element.Task.This);
    }
    
    [Test]
    public void Returns_WithMultipleSteps_ShouldBeExpected()
    {
        var spec = new Spec(Element.Tag);
        spec.Steps.Add(new Filter());
        spec.Steps.Add(new Select("TagName"));
        spec.Steps.Add(new Filter());

        var returns = spec.Returns;

        returns.Should().BeEquivalentTo(Property.This(typeof(TagName)), o => o.IgnoringCyclicReferences());
    }

    [Test]
    public void AddStep_ValidStep_ShouldBeExpected()
    {
        var spec = new Spec(Element.Tag);

        spec.Steps.Add(new Filter());

        spec.Steps.Should().HaveCount(1);
    }

    [Test]
    public void FluentBuild_WhenCalled_ShouldUpdateAsExpected()
    {
        var spec = Spec.Configure(s =>
        {
            s.Query(Element.Tag);
            s.Where("Name", Operation.Containing, "Test");
            s.Select("Value");
            s.Verify("This", Operation.EqualTo, 4);
        });

        spec.Element.Should().Be(Element.Tag);
        spec.Steps.Should().HaveCount(3);
        spec.Steps.First().As<Filter>().Criteria.Should().HaveCount(1);
        spec.Steps.Last().As<Verify>().Criteria.Should().HaveCount(1);
    }

    [Test]
    public void Duplicate_WhenCalled_ShouldBeNewInstanceBuEquivalent()
    {
        var spec = Spec.Configure(s =>
        {
            s.Query(Element.Tag);
            s.Where("Name", Operation.Containing, "Test");
            s.Verify("Value", Operation.EqualTo, 4);
        });

        var duplicate = spec.Duplicate();

        duplicate.Should().NotBeSameAs(spec);
        duplicate.Element.Should().BeEquivalentTo(spec.Element);
        duplicate.Steps.Should().BeEquivalentTo(spec.Steps);
    }

    [Test]
    public void GetCriteria_WhenCalled_ShouldBeExpected()
    {
        var spec = Spec.Configure(s =>
        {
            s.Query(Element.Tag);
            s.Where("Name", Operation.Containing, "Test");
            s.Verify("Value", Operation.EqualTo, 4);
        });

        var criteria = spec.GetAllCriteria();

        criteria.Should().HaveCount(2);
    }

    [Test]
    public async Task RunAsync_Default_ShouldReturnNoneDueToNoVerifications()
    {
        var spec = new Spec();
        var content = await L5X.LoadAsync(Known.Test);

        var verifications = await spec.RunAsync(content);

        verifications.Should().BeEmpty();
    }

    [Test]
    public async Task RunAsync_ValidSpec_ShouldReturnSuccess()
    {
        var spec = new Spec();
        var content = await L5X.LoadAsync(Known.Test);

        spec.Query(Element.Tag)
            .Where("Name", Operation.Containing, "Test")
            .Verify("DataType", Negation.Not, Operation.Void);

        var results = (await spec.RunAsync(content)).ToList();

        results.Should().NotBeEmpty();
        results.Should().AllSatisfy(e => e.Result.Should().Be(ResultState.Passed));
    }

    [Test]
    public async Task RunAsync_InhibitedIsFalse_ShouldReturnSuccess()
    {
        var spec = new Spec();
        var content = await L5X.LoadAsync(Known.Test);
        spec.Query(Element.Module).Verify("Inhibited", Operation.EqualTo, false);

        var verifications = (await spec.RunAsync(content)).ToList();

        verifications.Should().AllSatisfy(e => e.Result.Should().Be(ResultState.Passed));
        verifications.Should().NotBeEmpty();
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
            s.Query(Element.Tag);
            s.Where("Name", Operation.Containing, "Test");
            s.Verify("DataType", Negation.Not, Operation.Void);
        });

        var json = JsonSerializer.Serialize(spec, Options);

        return Verify(json, VerifySettings);
    }

    [Test]
    public Task Serialize_ConfiguredSpecWithSelect_ShouldBeVerified()
    {
        var spec = Spec.Configure(s =>
        {
            s.Query(Element.Tag);
            s.Where("Name", Operation.Containing, "Test");
            s.Select("Members");
            s.Verify("DataType", Negation.Not, Operation.Void);
        });

        var json = JsonSerializer.Serialize(spec, Options);

        return Verify(json, VerifySettings);
    }

    [Test]
    public Task Serialize_ConfiguredSpecWithRange_ShouldBeVerified()
    {
        var spec = Spec.Configure(s =>
        {
            s.Query(Element.Tag);
            s.Where("Name", Operation.Containing, "Test");
            s.Verify("Value", Negation.Is, Operation.Between, new Range(1, 10));
        });

        var json = JsonSerializer.Serialize(spec, Options);

        return Verify(json, VerifySettings);
    }

    [Test]
    public void Deserialize_WhenCalled_ShouldBeExpected()
    {
        var spec = Spec.Configure(s =>
        {
            s.Query(Element.Tag);
            s.Where("Name", Operation.Containing, "Test");
            s.Verify("DataType", Negation.Not, Operation.Void);
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
            s.Query(Element.Tag);
            s.Where("Name", Operation.Containing, "Test");
            s.Verify("Value", Negation.Is, Operation.Between, new Range(1, 10));
        });
        var data = JsonSerializer.Serialize(spec);

        var result = JsonSerializer.Deserialize<Spec>(data);

        result.Should().BeEquivalentTo(spec);
    }

    [DotMemoryUnit(FailIfRunWithoutSupport = false)]
    [Test]
    public void CheckForMemoryLeaksAgainstSpec()
    {
        var isolator = new Action(() =>
        {
            var content = L5X.Load(Known.Test);
            var spec = Spec.Configure(c =>
            {
                c.Query(Element.Module);
                c.Verify("Inhibited", Negation.Is, Operation.EqualTo, false);
            });

            var evaluations = spec.Run(content);
            evaluations.Should().AllSatisfy(e => e.Result.Should().Be(ResultState.Passed));
        });

        isolator();

        GC.Collect();
        GC.WaitForFullGCComplete();

        // Assert L5X is removed from memory
        dotMemory.Check(memory => memory.GetObjects(where => where.Type.Is<L5X>()).ObjectsCount.Should().Be(0));
        dotMemory.Check(memory => memory.GetObjects(where => where.Type.Is<Spec>()).ObjectsCount.Should().Be(0));
    }
}