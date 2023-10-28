using FluentAssertions;
using L5Spex.Engine.Enumerations;
using L5Spex.Engine.Operations;

namespace L5Spex.Engine.Tests.CriterionTests;

[TestFixture]
public class CriterionTagTests
{
    [Test]
    public void Evaluate_TagNameEqualTo_IsEqualTo_ShouldBeTrue()
    {
        var criterion = new Criterion(typeof(Tag), "Name", Operation.EqualTo, "Test");
        var tag = new Tag {Name = "Test"};
        
        var evaluation = criterion.Evaluate(tag);

        evaluation.Result.Should().Be(ResultType.Passed);
    }
    
    [Test]
    public void Evaluate_TagValueEqualTo_IsEqualTo_ShouldBeTrue()
    {
        var criterion = new Criterion(typeof(Tag), "Value", Operation.EqualTo, 1000);
        var tag = new Tag {Name = "Test", Value = 1000};
        
        var evaluation = criterion.Evaluate(tag);

        evaluation.Result.Should().Be(ResultType.Passed);
    }
    
    [Test]
    public void Evaluate_TagRadixEqualTo_IsEqualTo_ShouldBeTrue()
    {
        var criterion = new Criterion(typeof(Tag), "Radix", Operation.EqualTo, Radix.Decimal);
        var tag = new Tag {Name = "Test", Value = 1000};
        
        var evaluation = criterion.Evaluate(tag);

        evaluation.Result.Should().Be(ResultType.Passed);
    }
    
    [Test]
    public void Evaluate_TagNestedProperty_IsEqualTo_ShouldBeTrue()
    {
        var criterion = new Criterion(typeof(Tag), "Radix.Name", Operation.EqualTo, "Decimal");
        var tag = new Tag {Name = "Test", Value = 1000};
        
        var evaluation = criterion.Evaluate(tag);

        evaluation.Result.Should().Be(ResultType.Passed);
    }

    [Test]
    public void For_ValidArguments_ShouldHaveExpectedResult()
    {
        var criterion = Criterion.For<Tag>("Name", Operation.EqualTo, "Test");
        var tag = new Tag {Name = "Test", Value = 1000};

        var evaluation = criterion.Evaluate(tag);

        evaluation.Result.Should().Be(ResultType.Passed);
    }

    [Test]
    public void Evaluate_TagName_ExpectedBehavior()
    {
        
    }
}