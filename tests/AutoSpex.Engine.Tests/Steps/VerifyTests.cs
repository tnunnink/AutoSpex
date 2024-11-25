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
    public void Add_Default_ShouldBeExpectedCount()
    {
        var step = new Verify();

        step.Add();

        step.Criteria.Should().HaveCount(1);
    }

    [Test]
    public void Add_Default_ShouldNotBeNull()
    {
        var step = new Verify();

        var criterion = step.Add();

        criterion.Should().NotBeNull();
    }

    [Test]
    public void Add_Criterion_ShouldBeExpectedCount()
    {
        var step = new Verify();

        step.Add(new Criterion("TagName", Operation.Containing, "this is a test"));

        step.Criteria.Should().HaveCount(1);
    }

    [Test]
    public void Remove_NoCriteria_ShouldBeEmpty()
    {
        var step = new Verify();

        step.Remove(new Criterion());

        step.Criteria.Should().BeEmpty();
    }

    [Test]
    public void Remove_SingleCriteria_ShouldBeEmpty()
    {
        var step = new Verify();
        var criterion = step.Add();

        step.Remove(criterion);

        step.Criteria.Should().BeEmpty();
    }

    [Test]
    public void Remove_MultipleCriteria_ShouldHaveExpectedCount()
    {
        var step = new Verify();
        var first = step.Add();
        step.Add();
        step.Add();

        step.Remove(first);

        step.Criteria.Should().HaveCount(2);
    }

    [Test]
    public void Remove_ConfigredCriterion_ShouldBeEmpty()
    {
        var step = new Verify();
        var criterion = new Criterion("TagName", Operation.Containing, "this is a test");
        step.Add(criterion);

        step.Remove(criterion);

        step.Criteria.Should().BeEmpty();
    }

    [Test]
    public void Remove_MultipleConfigredCriterion_ShouldHaveExpectedCount()
    {
        var step = new Verify();
        var criterion = new Criterion("TagName", Operation.Containing, "this is a test");
        step.Add(criterion);
        step.Add(criterion);
        step.Add(criterion);

        step.Remove(criterion);

        step.Criteria.Should().HaveCount(2);
    }

    [Test]
    public void Move_InvalidIndex_ShouldThrowIndex()
    {
        var step = new Verify();

        FluentActions.Invoking(() => step.Move(-1, 2)).Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void Move_InvalidNewIndex_ShouldBeExpected()
    {
        var step = new Verify();
        step.Add();
        step.Add();
        step.Add();

        FluentActions.Invoking(() => step.Move(1, 5)).Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void Move_ValidIndexLowerToHigher_ShouldBeExpected()
    {
        var step = new Verify();
        var first = step.Add();
        var second = step.Add();
        var third = step.Add();

        step.Move(1, 2);

        step.Criteria.Should().ContainInOrder(first, third, second);
    }

    [Test]
    public void Move_ValidIndexHigherToLower_ShouldBeExpected()
    {
        var step = new Verify();
        var first = step.Add();
        var second = step.Add();
        var third = step.Add();

        step.Move(2, 0);

        step.Criteria.Should().ContainInOrder(third, first, second);
    }

    [Test]
    public void Move_ValidIndexSameIndex_ShouldBeExpected()
    {
        var step = new Verify();
        var first = step.Add();
        var second = step.Add();
        var third = step.Add();

        step.Move(1, 1);

        step.Criteria.Should().ContainInOrder(first, second, third);
    }

    [Test]
    public void Clear_NoCriteria_ShouldBeEmpty()
    {
        var step = new Verify();

        step.Clear();

        step.Criteria.Should().BeEmpty();
    }

    [Test]
    public void Clear_ManyCriteria_ShouldBeEmpty()
    {
        var step = new Verify();
        step.Add();
        step.Add();
        step.Add();

        step.Clear();

        step.Criteria.Should().BeEmpty();
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
        step.Add(new Criterion("Name", Operation.Containing, "Test"));

        var results = step.Process(tags).ToList();

        results.Should().HaveCount(3);
        results.Cast<Evaluation>().Should().AllSatisfy(x => x.Result.Should().Be(ResultState.Passed));
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
        step.Add(new Criterion("TagName", Operation.Containing, "this is a test"));
        step.Add(new Criterion("Value", Operation.EqualTo, 123));

        var json = JsonSerializer.Serialize(step);

        return VerifyJson(json);
    }

    [Test]
    public Task Serialize_ConfiguredTypeAndCriteriaAsStep_ShouldBeVerified()
    {
        var step = new Verify();
        step.Add(new Criterion("TagName", Operation.Containing, "this is a test"));
        step.Add(new Criterion("Value", Operation.EqualTo, 123));

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
        expected.Add(new Criterion("TagName", Operation.Containing, "this is a test"));
        expected.Add(new Criterion("Value", Operation.EqualTo, 123));
        var json = JsonSerializer.Serialize(expected);

        var result = JsonSerializer.Deserialize<Verify>(json);

        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Deserialize_ConfiguredTypeAndCriteriaAsStep_ShouldBeExpected()
    {
        var expected = new Verify();
        expected.Add(new Criterion("TagName", Operation.Containing, "this is a test"));
        expected.Add(new Criterion("Value", Operation.EqualTo, 123));
        var step = expected as Step;
        var json = JsonSerializer.Serialize(step);

        var result = JsonSerializer.Deserialize<Verify>(json);

        result.Should().BeEquivalentTo(expected);
    }
}