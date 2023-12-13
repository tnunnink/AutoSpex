using FluentAssertions;
using AutoSpex.Engine.Operations;

namespace AutoSpex.Engine.Tests.CriterionTests;

[TestFixture]
public class CriterionTagTests
{
    [Test]
    public void Evaluate_TagNameEqualTo_IsEqualTo_ShouldBeTrue()
    {
        var criterion = new Criterion(Element.Tag, "Name", Operation.EqualTo, "Test");
        var tag = new Tag {Name = "Test"};
        
        var evaluation = criterion.Evaluate(tag);

        evaluation.Result.Should().Be(ResultType.Passed);
    }
    
    [Test]
    public void Evaluate_TagValueEqualTo_IsEqualTo_ShouldBeTrue()
    {
        var criterion = new Criterion(Element.Tag, "Value", Operation.EqualTo, "1000");
        var tag = new Tag {Name = "Test", Value = 1000};
        
        var evaluation = criterion.Evaluate(tag);

        evaluation.Result.Should().Be(ResultType.Passed);
    }
    
    [Test]
    public void Evaluate_TagRadixEqualTo_IsEqualTo_ShouldBeTrue()
    {
        var criterion = new Criterion(Element.Tag, "Radix", Operation.EqualTo, "Decimal");
        var tag = new Tag {Name = "Test", Value = 1000};
        
        var evaluation = criterion.Evaluate(tag);

        evaluation.Result.Should().Be(ResultType.Passed);
    }
    
    [Test]
    public void Evaluate_TagNestedProperty_IsEqualTo_ShouldBeTrue()
    {
        var criterion = new Criterion(Element.Tag, "Radix.Name", Operation.EqualTo, "Decimal");
        var tag = new Tag {Name = "Test", Value = 1000};
        
        var evaluation = criterion.Evaluate(tag);

        evaluation.Result.Should().Be(ResultType.Passed);
    }

    [Test]
    public void For_ValidArguments_ShouldHaveExpectedResult()
    {
        var criterion = new Criterion(Element.Tag, "Name", Operation.EqualTo, "Test");
        var tag = new Tag {Name = "Test", Value = 1000};

        var evaluation = criterion.Evaluate(tag);

        evaluation.Result.Should().Be(ResultType.Passed);
    }

    [Test]
    public void Evaluate_TagName_ExpectedBehavior()
    {
        
    }
}