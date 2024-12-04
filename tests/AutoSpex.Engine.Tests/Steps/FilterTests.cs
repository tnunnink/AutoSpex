// ReSharper disable UseObjectOrCollectionInitializer

using System.Text.Json;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Engine.Tests.Steps;

[TestFixture]
public class FilterTests
{
    [Test]
    public void New_Default_ShouldBeExpected()
    {
        var filter = new Filter();

        filter.Match.Should().Be(Match.All);
        filter.Criteria.Should().BeEmpty();
    }

    [Test]
    public void Match_Any_ShouldBeExpected()
    {
        var filter = new Filter();

        filter.Match = Match.Any;

        filter.Match.Should().Be(Match.Any);
    }

    [Test]
    public void Add_Default_ShouldBeExpectedCount()
    {
        var filter = new Filter();

        filter.Add();

        filter.Criteria.Should().HaveCount(1);
    }

    [Test]
    public void Add_Default_ShouldNotBeNull()
    {
        var filter = new Filter();

        var criterion = filter.Add();

        criterion.Should().NotBeNull();
    }

    [Test]
    public void Add_Criterion_ShouldBeExpectedCount()
    {
        var filter = new Filter();

        filter.Criteria.Add(new Criterion("TagName", Operation.Containing, "this is a test"));

        filter.Criteria.Should().HaveCount(1);
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
        var filter = new Filter();
        filter.Criteria.Add(new Criterion("Name", Operation.Containing, "Test"));

        var results = filter.Process(tags).ToList();

        results.Should().HaveCount(2);
        results.Cast<Tag>().Should().AllSatisfy(t => t.Name.Should().Contain("Test"));
    }

    [Test]
    public Task Serialize_DefaultInstance_ShouldBeVerified()
    {
        var filter = new Filter();

        var json = JsonSerializer.Serialize(filter);

        return VerifyJson(json);
    }

    [Test]
    public Task Serialize_ConfiguredTypeAndCriteria_ShouldBeVerified()
    {
        var filter = new Filter();
        filter.Criteria.Add(new Criterion("TagName", Operation.Containing, "this is a test"));
        filter.Criteria.Add(new Criterion("Value", Operation.EqualTo, 123));

        var json = JsonSerializer.Serialize(filter);

        return VerifyJson(json);
    }

    [Test]
    public Task Serialize_ConfiguredTypeAndCriteriaAsStep_ShouldBeVerified()
    {
        var filter = new Filter();
        filter.Criteria.Add(new Criterion("TagName", Operation.Containing, "this is a test"));
        filter.Criteria.Add(new Criterion("Value", Operation.EqualTo, 123));
        var step = filter as Step;

        var json = JsonSerializer.Serialize(step);

        return VerifyJson(json);
    }

    [Test]
    public void Deserialize_Default_ShouldBeExpected()
    {
        var expected = new Filter();
        var json = JsonSerializer.Serialize(expected);

        var result = JsonSerializer.Deserialize<Filter>(json);

        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Deserialize_ConfiguredTypeAndMatch_ShouldBeExpected()
    {
        var expected = new Filter();
        expected.Match = Match.Any;
        var json = JsonSerializer.Serialize(expected);

        var result = JsonSerializer.Deserialize<Filter>(json);

        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Deserialize_ConfiguredTypeAndCriteria_ShouldBeExpected()
    {
        var expected = new Filter();
        expected.Criteria.Add(new Criterion("TagName", Operation.Containing, "this is a test"));
        expected.Criteria.Add(new Criterion("Value", Operation.EqualTo, 123));
        var json = JsonSerializer.Serialize(expected);

        var result = JsonSerializer.Deserialize<Filter>(json);

        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Deserialize_ConfiguredTypeAndCriteriaAsStep_ShouldBeExpected()
    {
        var expected = new Filter();
        expected.Criteria.Add(new Criterion("TagName", Operation.Containing, "this is a test"));
        expected.Criteria.Add(new Criterion("Value", Operation.EqualTo, 123));
        var step = expected as Step;
        var json = JsonSerializer.Serialize(step);

        var result = JsonSerializer.Deserialize<Filter>(json);

        result.Should().BeEquivalentTo(expected);
    }
}