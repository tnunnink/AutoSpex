using System.Linq.Expressions;

namespace AutoSpex.Engine.Tests.Specifications;

[TestFixture]
public class CriterionConceptTests
{
    [Test]
    public void New_DefaultConstructor_ShouldHaveExpectedDefaults()
    {
        var criterion = new Criterion();

        criterion.Should().NotBeNull();
        criterion.Property.Should().BeEmpty();
        criterion.Operation.Should().Be(Operation.None);
        criterion.Arguments.Should().BeEmpty();
    }
    
    [Test]
    public void Build_SimpleOverloads_ShouldHaveExpected()
    {
        var criterion = new Criterion("Test", Operation.Between, 0, 100);

        criterion.Property.Should().Be("Test");
        criterion.Operation.Should().Be(Operation.Between);
        criterion.Arguments.Should().HaveCount(2);
    }
    
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