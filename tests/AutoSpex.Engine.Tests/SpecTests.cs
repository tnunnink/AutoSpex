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
        spec.Steps.Should().HaveCount(2);
        spec.Steps.First().Should().BeOfType<Query>();
        spec.Steps.First().As<Query>().Element.Should().Be(Element.Default);
        spec.Steps.Last().Should().BeOfType<Verify>();
        spec.Steps.Last().As<Verify>().Criteria.Should().BeEmpty();
    }

    [Test]
    public void New_ValidElement_ShouldHaveExpectedElementForQueryStep()
    {
        var spec = new Spec(Element.Tag);

        spec.Steps.First().As<Query>().Element.Should().Be(Element.Tag);
    }

    [Test]
    public void AddStep_QueryStepType_ShouldThrowException()
    {
        var spec = new Spec(Element.Tag);

        FluentActions.Invoking(() => spec.AddStep(new Query(Element.Rung))).Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void AddStep_VerifyStepType_ShouldThrowException()
    {
        var spec = new Spec(Element.Tag);

        FluentActions.Invoking(() => spec.AddStep(new Verify())).Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void AddStep_FilterStepType_ShouldHaveExpectedStepCount()
    {
        var spec = new Spec(Element.Tag);

        spec.AddStep(new Filter());

        spec.Steps.Should().HaveCount(3);
    }

    [Test]
    public void AddStep_FilterStepType_ShouldHaveCorrectOrder()
    {
        var spec = new Spec(Element.Tag);

        spec.AddStep(new Filter());

        var steps = spec.Steps.ToList();
        steps[0].Should().BeOfType<Query>();
        steps[1].Should().BeOfType<Filter>();
        steps[2].Should().BeOfType<Verify>();
    }

    [Test]
    public void AddStep_SelectStepType_ShouldHaveExpectedStepCount()
    {
        var spec = new Spec(Element.Tag);

        spec.AddStep(new Select());

        spec.Steps.Should().HaveCount(3);
    }

    [Test]
    public void AddStep_SelectStepType_ShouldHaveCorrectOrder()
    {
        var spec = new Spec(Element.Tag);

        spec.AddStep(new Select());

        var steps = spec.Steps.ToList();
        steps[0].Should().BeOfType<Query>();
        steps[1].Should().BeOfType<Select>();
        steps[2].Should().BeOfType<Verify>();
    }

    [Test]
    public void AddStep_MultipleSteps_ShouldHaveExpectedCountAndOrder()
    {
        var spec = new Spec(Element.Tag);

        spec.AddStep(new Filter());
        spec.AddStep(new Select());
        spec.AddStep(new Filter());
        spec.AddStep(new Filter());

        var steps = spec.Steps.ToList();
        steps.Count.Should().Be(6);
        steps[0].Should().BeOfType<Query>();
        steps[1].Should().BeOfType<Filter>();
        steps[2].Should().BeOfType<Select>();
        steps[3].Should().BeOfType<Filter>();
        steps[4].Should().BeOfType<Filter>();
        steps[5].Should().BeOfType<Verify>();
    }

    [Test]
    public void RemoveStep_QueryStep_ShouldThrowInvalidOperationException()
    {
        var spec = new Spec(Element.Tag);

        FluentActions.Invoking(() => spec.RemoveStep(new Query())).Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void RemoveStep_VerifyStep_ShouldThrowInvalidOperationException()
    {
        var spec = new Spec(Element.Tag);

        FluentActions.Invoking(() => spec.RemoveStep(new Verify())).Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void RemoveStep_FilterStepType_ShouldHaveExpectedStepCount()
    {
        var spec = new Spec(Element.Tag);
        var step = new Filter();
        spec.AddStep(step);

        spec.RemoveStep(step);

        spec.Steps.Should().HaveCount(2);
    }

    [Test]
    public void RemoveStep_FilterStepType_ShouldHaveCorrectOrder()
    {
        var spec = new Spec(Element.Tag);
        var step = new Select();
        spec.AddStep(step);

        spec.RemoveStep(step);

        var steps = spec.Steps.ToList();
        steps[0].Should().BeOfType<Query>();
        steps[1].Should().BeOfType<Verify>();
    }

    [Test]
    public void RemoveStep_SelectStepType_ShouldHaveExpectedStepCount()
    {
        var spec = new Spec(Element.Tag);
        var step = new Select();
        spec.AddStep(step);

        spec.RemoveStep(step);

        spec.Steps.Should().HaveCount(2);
    }

    [Test]
    public void RemoveStep_SelectStepType_ShouldHaveCorrectOrder()
    {
        var spec = new Spec(Element.Tag);
        var step = new Filter();
        spec.AddStep(step);

        spec.RemoveStep(step);

        var steps = spec.Steps.ToList();
        steps[0].Should().BeOfType<Query>();
        steps[1].Should().BeOfType<Verify>();
    }

    [Test]
    public void FluentBuild_WhenCalled_ShouldUpdateAsExpected()
    {
        var spec = new Spec();

        spec.Fetch(Element.Tag)
            .Where("Name", Operation.Containing, "Test")
            .Confirm("Value", Operation.EqualTo, 4);

        var steps = spec.Steps.ToList();
        steps.Should().HaveCount(3);
        steps[0].As<Query>().Element.Should().Be(Element.Tag);
        steps[1].As<Filter>().Criteria.Should().HaveCount(1);
        steps[2].As<Verify>().Criteria.Should().HaveCount(1);
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

        spec.Fetch(Element.Tag)
            .Where("Name", Operation.Containing, "Test")
            .Confirm("DataType", Negation.Not, Operation.Void);

        var verification = await spec.RunAsync(content);

        verification.Result.Should().Be(ResultState.Passed);
        verification.Evaluations.Should().NotBeEmpty();
    }

    [Test]
    public async Task RunAsync_InhibitedIsFalse_ShouldReturnSuccess()
    {
        var spec = new Spec();
        var content = L5X.Load(Known.Test);
        spec.Fetch(Element.Module).Confirm("Inhibited", Operation.EqualTo, false);

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
            s.Fetch(Element.Tag);
            s.Where("Name", Operation.Containing, "Test");
            s.Confirm("DataType", Negation.Not, Operation.Void);
        });

        var json = JsonSerializer.Serialize(spec, Options);

        return Verify(json, VerifySettings);
    }

    [Test]
    public Task Serialize_ConfiguredSpecWithRange_ShouldBeVerified()
    {
        var spec = Spec.Configure(s =>
        {
            s.Fetch(Element.Tag);
            s.Where("Name", Operation.Containing, "Test");
            s.Confirm("Value", Negation.Is, Operation.Between, new Range(1, 10));
        });

        var json = JsonSerializer.Serialize(spec, Options);

        return Verify(json, VerifySettings);
    }

    [Test]
    public void Deserialize_WhenCalled_ShouldBeExpected()
    {
        var spec = Spec.Configure(s =>
        {
            s.Fetch(Element.Tag);
            s.Where("Name", Operation.Containing, "Test");
            s.Confirm("DataType", Negation.Not, Operation.Void);
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
            s.Fetch(Element.Tag);
            s.Where("Name", Operation.Containing, "Test");
            s.Confirm("Value", Negation.Is, Operation.Between, new Range(1, 10));
        });
        var data = JsonSerializer.Serialize(spec);

        var result = JsonSerializer.Deserialize<Spec>(data);

        result.Should().BeEquivalentTo(spec);
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
                c.Fetch(Element.Module);
                c.Confirm("Inhibited", Negation.Is, Operation.EqualTo, false);
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