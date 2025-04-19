using System.Text.Json;
using Task = System.Threading.Tasks.Task;

// ReSharper disable UseObjectOrCollectionInitializer

namespace AutoSpex.Engine.Tests.Steps;

[TestFixture]
public class VerifyTests
{
    [Test]
    public void New_Default_ShouldBeExpected()
    {
        var step = new Verify();

        step.Criteria.Should().BeEmpty();
    }

    [Test]
    public void New_WithCriterion_ShouldBeExpectedCount()
    {
        var filter = new Filter(new Criterion());

        filter.Criteria.Should().HaveCount(1);
    }

    [Test]
    public void Process_ValidElements_ShouldBeExpected()
    {
        var tags = new List<Tag>
        {
            new("TestTag", 123),
            new("AnotherTestTag", new TIMER()),
            new("MyTestTag", "This is a value")
        };
        var step = new Verify();
        step.Criteria.Add(new Criterion("Name", Operation.Containing, "Test"));

        var results = step.Process(tags).ToList();

        results.Should().HaveCount(3);
        results.Cast<Verification>().Should().AllSatisfy(x => x.Result.Should().Be(ResultState.Passed));
    }

    [Test]
    public Task Serialize_DefaultInstance_ShouldBeVerified()
    {
        var step = new Verify();

        var json = JsonSerializer.Serialize(step);

        return VerifyJson(json);
    }

    [Test]
    public Task Serialize_ConfiguredTypeAndCriteria_ShouldBeVerified()
    {
        var step = new Verify();
        step.Criteria.Add(new Criterion("TagName", Operation.Containing, "this is a test"));
        step.Criteria.Add(new Criterion("Value", Operation.EqualTo, 123));

        var json = JsonSerializer.Serialize(step);

        return VerifyJson(json);
    }

    [Test]
    public Task Serialize_ConfiguredTypeAndCriteriaAsStep_ShouldBeVerified()
    {
        var step = new Verify();
        step.Criteria.Add(new Criterion("TagName", Operation.Containing, "this is a test"));
        step.Criteria.Add(new Criterion("Value", Operation.EqualTo, 123));

        var json = JsonSerializer.Serialize(step as Step);

        return VerifyJson(json);
    }

    [Test]
    public void Deserialize_Default_ShouldBeExpected()
    {
        var expected = new Verify();
        var json = JsonSerializer.Serialize(expected);

        var result = JsonSerializer.Deserialize<Verify>(json);

        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Deserialize_ConfiguredTypeAndCriteria_ShouldBeExpected()
    {
        var expected = new Verify();
        expected.Criteria.Add(new Criterion("TagName", Operation.Containing, "this is a test"));
        expected.Criteria.Add(new Criterion("Value", Operation.EqualTo, 123));
        var json = JsonSerializer.Serialize(expected);

        var result = JsonSerializer.Deserialize<Verify>(json);

        result.Should().BeEquivalentTo(expected);
    }
}