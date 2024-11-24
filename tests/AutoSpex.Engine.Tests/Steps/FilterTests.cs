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

        filter.Add(new Criterion(Element.Tag.Property("TagName"), Operation.Containing, "this is a test"));

        filter.Criteria.Should().HaveCount(1);
    }

    [Test]
    public void Remove_NoCriteria_ShouldBeEmpty()
    {
        var filter = new Filter();

        filter.Remove(new Criterion());

        filter.Criteria.Should().BeEmpty();
    }

    [Test]
    public void Remove_SingleCriteria_ShouldBeEmpty()
    {
        var filter = new Filter();
        var criterion = filter.Add();

        filter.Remove(criterion);

        filter.Criteria.Should().BeEmpty();
    }

    [Test]
    public void Remove_MultipleCriteria_ShouldHaveExpectedCount()
    {
        var filter = new Filter();
        var first = filter.Add();
        filter.Add();
        filter.Add();

        filter.Remove(first);

        filter.Criteria.Should().HaveCount(2);
    }

    [Test]
    public void Remove_ConfigredCriterion_ShouldBeEmpty()
    {
        var filter = new Filter();
        var criterion = new Criterion(Element.Tag.Property("TagName"), Operation.Containing, "this is a test");
        filter.Add(criterion);

        filter.Remove(criterion);

        filter.Criteria.Should().BeEmpty();
    }

    [Test]
    public void Remove_MultipleConfigredCriterion_ShouldHaveExpectedCount()
    {
        var filter = new Filter();
        var criterion = new Criterion(Element.Tag.Property("TagName"), Operation.Containing, "this is a test");
        filter.Add(criterion);
        filter.Add(criterion);
        filter.Add(criterion);

        filter.Remove(criterion);

        filter.Criteria.Should().HaveCount(2);
    }

    [Test]
    public void Move_InvalidIndex_ShouldThrowIndex()
    {
        var filter = new Filter();

        FluentActions.Invoking(() => filter.Move(-1, 2)).Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void Move_InvalidNewIndex_ShouldBeExpected()
    {
        var filter = new Filter();
        filter.Add();
        filter.Add();
        filter.Add();

        FluentActions.Invoking(() => filter.Move(1, 5)).Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void Move_ValidIndexLowerToHigher_ShouldBeExpected()
    {
        var filter = new Filter();
        var first = filter.Add();
        var second = filter.Add();
        var third = filter.Add();

        filter.Move(1, 2);

        filter.Criteria.Should().ContainInOrder(first, third, second);
    }

    [Test]
    public void Move_ValidIndexHigherToLower_ShouldBeExpected()
    {
        var filter = new Filter();
        var first = filter.Add();
        var second = filter.Add();
        var third = filter.Add();

        filter.Move(2, 0);

        filter.Criteria.Should().ContainInOrder(third, first, second);
    }

    [Test]
    public void Move_ValidIndexSameIndex_ShouldBeExpected()
    {
        var filter = new Filter();
        var first = filter.Add();
        var second = filter.Add();
        var third = filter.Add();

        filter.Move(1, 1);

        filter.Criteria.Should().ContainInOrder(first, second, third);
    }

    [Test]
    public void Clear_NoCriteria_ShouldBeEmpty()
    {
        var filter = new Filter();

        filter.Clear();

        filter.Criteria.Should().BeEmpty();
    }

    [Test]
    public void Clear_ManyCriteria_ShouldBeEmpty()
    {
        var filter = new Filter();
        filter.Add();
        filter.Add();
        filter.Add();

        filter.Clear();

        filter.Criteria.Should().BeEmpty();
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
        filter.Add(new Criterion(Element.Tag.Property("Name"), Operation.Containing, "Test"));

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
        filter.Add(new Criterion(Element.Tag.Property("TagName"), Operation.Containing, "this is a test"));
        filter.Add(new Criterion(Element.Tag.Property("Value"), Operation.EqualTo, 123));

        var json = JsonSerializer.Serialize(filter);

        return VerifyJson(json);
    }

    [Test]
    public Task Serialize_ConfiguredTypeAndCriteriaAsStep_ShouldBeVerified()
    {
        var filter = new Filter();
        filter.Add(new Criterion(Element.Tag.Property("TagName"), Operation.Containing, "this is a test"));
        filter.Add(new Criterion(Element.Tag.Property("Value"), Operation.EqualTo, 123));
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
        expected.Add(new Criterion(Element.Tag.Property("TagName"), Operation.Containing, "this is a test"));
        expected.Add(new Criterion(Element.Tag.Property("Value"), Operation.EqualTo, 123));
        var json = JsonSerializer.Serialize(expected);

        var result = JsonSerializer.Deserialize<Filter>(json);

        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Deserialize_AsStep_ShouldBeExpected()
    {
        var expected = (Step)new Filter();
        var json = JsonSerializer.Serialize(expected);

        var result = JsonSerializer.Deserialize<Step>(json);

        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Deserialize_ConfiguredTypeAndCriteriaAsStep_ShouldBeExpected()
    {
        var expected = new Filter();
        expected.Add(new Criterion(Element.Tag.Property("TagName"), Operation.Containing, "this is a test"));
        expected.Add(new Criterion(Element.Tag.Property("Value"), Operation.EqualTo, 123));
        var step = expected as Step;
        var json = JsonSerializer.Serialize(step);

        var result = JsonSerializer.Deserialize<Filter>(json);

        result.Should().BeEquivalentTo(expected);
    }
}