using System.Linq.Expressions;

namespace AutoSpex.Engine.Tests.CriterionTests;

[TestFixture]
public class CriterionConceptTests
{
    [Test]
    public void New_DefaultConstructor_ShouldHaveExpectedDefaults()
    {
        var criterion = new Criterion();

        criterion.Should().NotBeNull();
        criterion.Property.Should().BeNull();
        criterion.Operation.Should().BeNull();
        criterion.Arguments.Should().BeEmpty();
    }

    [Test]
    public void Build_SimpleOverloads_ShouldHaveExpected()
    {
        var criterion = new Criterion(Element.Tag.Property("Value"), Operation.Between, 0, 100);

        criterion.Property.Should().Be("Value");
        criterion.Operation.Should().Be(Operation.Between);
        criterion.Arguments.Should().HaveCount(2);
    }

    [Test]
    public void Evaluate_SimpleImmediateProperty_ShouldBeExpectedResult()
    {
        var tag = new Tag {Name = "Test"};

        var criterion = new Criterion(Element.Tag.Property("Name"), Operation.Equal, "Test");

        var evaluation = criterion.Evaluate(tag);

        evaluation.Result.Should().Be(ResultState.Passed);
    }

    [Test]
    public void Evaluate_InvalidOperationForPropertyType_ShouldHaveFailedEvaluation()
    {
        var tag = new Tag {Name = "Test"};

        var criterion = new Criterion(Element.Tag.Property("Name"), Operation.Between, 1, 10);

        var evaluation = criterion.Evaluate(tag);

        evaluation.Result.Should().Be(ResultState.Error);
    }

    [Test]
    public void Evaluate_StringBetweenValidValues_ShouldHavePassedEvaluation()
    {
        var tag = new Tag {Name = "MyName"};

        var criterion = new Criterion(Element.Tag.Property("Name"), Operation.Between, "C", "T");

        var evaluation = criterion.Evaluate(tag);

        evaluation.Result.Should().Be(ResultState.Passed);
    }

    [Test]
    public void AsExpression_WhenCompiled_ShouldAlsoWork()
    {
        var tag = new Tag {Name = "Test"};

        var criterion = new Criterion(Element.Tag.Property("Name"), Operation.Equal, "Test");


        var expression = (Expression<Func<object?, bool>>) criterion;
        var func = expression.Compile();

        var result = func(tag);
        result.Should().BeTrue();
    }

    [Test]
    public void ToString_WhenCalled_ShouldNotBeEmpty()
    {
        var criterion = new Criterion(Element.Tag.Property("Name"), Operation.Equal, "Test");

        var result = criterion.ToString();

        result.Should().NotBeEmpty();
    }

    [Test]
    public void VariableArgument_WhenEvaluated_ShouldRunCorrectly()
    {
        var tag = new Tag {Name = "MyTestTag"};
        var variable = new Variable("MyVar", "Test");
        var criterion = new Criterion(Element.Tag.Property("Name"), Operation.Contains, variable);

        var eval = criterion.Evaluate(tag);

        eval.Result.Should().Be(ResultState.Passed);
    }

    [Test]
    public void VariableArgument_EnumStringValue_ShouldRunCorrectly()
    {
        var tag = new Tag {Name = "MyTestTag", Value = 123};
        var variable = new Variable("MyVar", "Decimal");
        var criterion = new Criterion(Element.Tag.Property("Radix"), Operation.Equal, variable);

        var eval = criterion.Evaluate(tag);

        eval.Result.Should().Be(ResultState.Passed);
    }

    [Test]
    public void VariableArgument_LogixTypeStringValue_ShouldRunCorrectly()
    {
        var tag = new Tag {Name = "TestTag", Value = 1.21f};
        var variable = new Variable("Value", "1.221");
        var criterion = new Criterion(Element.Tag.Property("Value"), Operation.GreaterThan, variable);
        
        var eval = criterion.Evaluate(tag);
        
        eval.Result.Should().Be(ResultState.Failed);
    }
    
    [Test]
    public void VariableArgument_LogixTypeFloatEquals_ShouldRunCorrectly()
    {
        var tag = new Tag {Name = "TestTag", Value = 1.2345f};
        var variable = new Variable("Value", 1.2345f);
        var criterion = new Criterion(Element.Tag.Property("Value"), Operation.Equal, variable);
        
        var eval = criterion.Evaluate(tag);
        
        eval.Result.Should().Be(ResultState.Passed);
    }
}