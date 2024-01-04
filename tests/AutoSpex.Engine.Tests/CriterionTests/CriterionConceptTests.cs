using System.Linq.Expressions;
using AutoSpex.Engine.Operations;

namespace AutoSpex.Engine.Tests.CriterionTests;

[TestFixture]
public class CriterionConceptTests
{
    [Test]
    public void Evaluate_SimpleImmediateProperty_ShouldBeExpectedResult()
    {
        var tag = new Tag {Name = "Test"};
        
        var criterion = new Criterion("Name", Operation.Equal, "Test");

        var evaluation = criterion.Evaluate(tag);

        evaluation.Result.Should().Be(ResultType.Passed);
    }

    [Test]
    public void Evaluate_InvalidOperationForPropertyType_ShouldHaveErrorEvaluation()
    {
        var tag = new Tag {Name = "Test"};
        
        var criterion = new Criterion("Name", Operation.Between, 1, 10);
        
        var evaluation = criterion.Evaluate(tag);
        
        evaluation.Result.Should().Be(ResultType.Error);
    }

    [Test]
    public void AsExpression_WhenCompiled_ShouldAlsoWork()
    {
        var tag = new Tag {Name = "Test"};
        
        var criterion = new Criterion("Name", Operation.Equal, "Test");
        

        var expression = (Expression<Func<object?, bool>>) criterion;
        var func = expression.Compile();

        var result = func(tag);
        result.Should().BeTrue();
    }
    
    [Test]
    public void ToString_WhenCalled_ShouldNotBeEmpty()
    {
        var criterion = new Criterion("Name", Operation.Equal, "Test");

        var result = criterion.ToString();
        
        result.Should().NotBeEmpty();
    }
}