using L5Spex.Engine.Enumerations;
using L5Spex.Engine.Operations;

namespace L5Spex.Engine.Tests.CriterionTests;

[TestFixture]
public class CriterionConceptTests
{
    [Test]
    public void Evaluate_SimpleImmediateProperty_ShouldBeExpectedResult()
    {
        var criterion = new Criterion(typeof(Tag), "Name", Operation.EqualTo, "Test");
        var tag = new Tag {Name = "Test"};

        var evaluation = criterion.Evaluate(tag);

        evaluation.Result.Should().Be(ResultType.Passed);
    }

    [Test]
    public void Evaluate_CollectionOfElementsForCount_ShouldBeExpectedResult()
    {
        var criterion = new Criterion(typeof(List<Tag>), "Count", Operation.GreaterThan, 0);
        var tags = new List<Tag> {new(), new(), new()};

        var evaluation = criterion.Evaluate(tags);

        evaluation.Result.Should().Be(ResultType.Passed);
    }

    [Test]
    public void Evaluate_InvalidOperationForPropertyType_ShouldHaveErrorEvaluation()
    {
        var criterion = new Criterion(typeof(Tag), "Name", Operation.Between, 1, 10);
        var tag = new Tag {Name = "Test"};

        var evaluation = criterion.Evaluate(tag);
        
        evaluation.Result.Should().Be(ResultType.Error);
    }

    [Test]
    public void ToString_WhenCalled_ShouldBeReadableLol()
    {
        var criterion = new Criterion(typeof(Tag), "Name", Operation.EqualTo, "Test");

        var result = criterion.ToString();

        result.Should().NotBeNullOrEmpty();
    }
}