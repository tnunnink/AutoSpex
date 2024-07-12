namespace AutoSpex.Engine.Tests.Criteria;

[TestFixture]
public class CriterionTagTests
{
    [Test]
    public void Evaluate_TagNameEqualTo_IsEqualTo_ShouldBeTrue()
    {
        var criterion = new Criterion(Element.Tag.Property("Name"), Operation.Equal, "Test");
        var tag = new Tag {Name = "Test"};
        
        var evaluation = criterion.Evaluate(tag);

        evaluation.Result.Should().Be(ResultState.Passed);
    }
    
    [Test]
    public void Evaluate_TagValueEqualTo_IsEqualTo_ShouldBeTrue()
    {
        var criterion = new Criterion(Element.Tag.Property("Value"), Operation.Equal, new INT(1000));
        var tag = new Tag {Name = "Test", Value = 1000};
        
        var evaluation = criterion.Evaluate(tag);

        evaluation.Result.Should().Be(ResultState.Passed);
    }
    
    [Test]
    public void Evaluate_TagRadixEqualTo_IsEqualTo_ShouldBeTrue()
    {
        var criterion = new Criterion(Element.Tag.Property("Radix"), Operation.Equal, Radix.Decimal);
        var tag = new Tag {Name = "Test", Value = 1000};
        
        var evaluation = criterion.Evaluate(tag);

        evaluation.Result.Should().Be(ResultState.Passed);
    }
    
    [Test]
    public void Evaluate_TagNestedProperty_IsEqualTo_ShouldBeTrue()
    {
        var criterion = new Criterion(Element.Tag.Property("Radix.Name"), Operation.Equal, "Decimal");
        var tag = new Tag {Name = "Test", Value = 1000};
        
        var evaluation = criterion.Evaluate(tag);

        evaluation.Result.Should().Be(ResultState.Passed);
    }

    [Test]
    public void For_ValidArguments_ShouldHaveExpectedResult()
    {
        var criterion = new Criterion(Element.Tag.Property("Name"), Operation.Equal, "Test");
        var tag = new Tag {Name = "Test", Value = 1000};

        var evaluation = criterion.Evaluate(tag);

        evaluation.Result.Should().Be(ResultState.Passed);
    }

    [Test]
    public void Evaluate_TagName_ExpectedBehavior()
    {
        
    }
}