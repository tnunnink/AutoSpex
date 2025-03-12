using System.Text.Json;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Engine.Tests.Steps;

[TestFixture]
public class CountTests
{
    [Test]
    public void New_Default_ShouldBeExpected()
    {
        var step = new Count();

        step.Match.Should().Be(Match.All);
        step.Criteria.Should().BeEmpty();
    }

    [Test]
    public void Match_Any_ShouldBeExpected()
    {
        // ReSharper disable once UseObjectOrCollectionInitializer
        var step = new Count();

        step.Match = Match.Any;

        step.Match.Should().Be(Match.Any);
    }

    [Test]
    public void New_WithCriterion_ShouldBeExpectedCount()
    {
        var step = new Count(new Criterion());

        step.Criteria.Should().HaveCount(1);
    }

    [Test]
    public void Add_Criterion_ShouldBeExpectedCount()
    {
        var step = new Count();

        step.Criteria.Add(new Criterion("TagName", Operation.Containing, "this is a test"));

        step.Criteria.Should().HaveCount(1);
    }

    [Test]
    public void Process_ValidElements_ShouldBeExpected()
    {
        var tags = new List<Tag>
        {
            new("TestTag", 123),
            new("AnotherTag", new TIMER()),
            new("MyTestTag", "This is a value")
        };
        var count = new Count();
        count.Criteria.Add(new Criterion("Name", Operation.Containing, "Test"));

        var results = count.Process(tags).ToList();

        results.Should().HaveCount(1);
        results.First().As<int>().Should().Be(2);
    }

    [Test]
    public Task Serialize_DefaultInstance_ShouldBeVerified()
    {
        var step = new Count();

        var json = JsonSerializer.Serialize(step);

        return VerifyJson(json);
    }

    [Test]
    public Task Serialize_ConfiguredTypeAndCriteria_ShouldBeVerified()
    {
        var step = new Count();
        step.Criteria.Add(new Criterion("TagName", Operation.Containing, "this is a test"));
        step.Criteria.Add(new Criterion("Value", Operation.EqualTo, 123));

        var json = JsonSerializer.Serialize(step);

        return VerifyJson(json);
    }

    [Test]
    public Task Serialize_ConfiguredTypeAndCriteriaAsStep_ShouldBeVerified()
    {
        var count = new Count();
        count.Criteria.Add(new Criterion("TagName", Operation.Containing, "this is a test"));
        count.Criteria.Add(new Criterion("Value", Operation.EqualTo, 123));
        var step = count as Step;

        var json = JsonSerializer.Serialize(step);

        return VerifyJson(json);
    }

    [Test]
    public void Deserialize_Default_ShouldBeExpected()
    {
        var expected = new Count();
        var json = JsonSerializer.Serialize(expected);

        var result = JsonSerializer.Deserialize<Count>(json);

        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Deserialize_ConfiguredTypeAndMatch_ShouldBeExpected()
    {
        // ReSharper disable once UseObjectOrCollectionInitializer
        var expected = new Count();
        expected.Match = Match.Any;
        var json = JsonSerializer.Serialize(expected);

        var result = JsonSerializer.Deserialize<Count>(json);

        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Deserialize_ConfiguredTypeAndCriteria_ShouldBeExpected()
    {
        var expected = new Count();
        expected.Criteria.Add(new Criterion("TagName", Operation.Containing, "this is a test"));
        expected.Criteria.Add(new Criterion("Value", Operation.EqualTo, 123));
        var json = JsonSerializer.Serialize(expected);

        var result = JsonSerializer.Deserialize<Count>(json);

        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Deserialize_ConfiguredTypeAndCriteriaAsStep_ShouldBeExpected()
    {
        var expected = new Count();
        expected.Criteria.Add(new Criterion("TagName", Operation.Containing, "this is a test"));
        expected.Criteria.Add(new Criterion("Value", Operation.EqualTo, 123));
        var step = expected as Step;
        var json = JsonSerializer.Serialize(step);

        var result = JsonSerializer.Deserialize<Count>(json);

        result.Should().BeEquivalentTo(expected);
    }
}