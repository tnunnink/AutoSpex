using System.Linq.Expressions;

namespace AutoSpex.Engine.Tests;

[TestFixture]
public class CriterionTests
{
    [Test]
    public void New_Default_ShouldHaveExpectedDefaults()
    {
        var criterion = new Criterion();

        criterion.Should().NotBeNull();
        criterion.Type.Should().Be(typeof(object));
        criterion.Property.Should().Be(Property.Default);
        criterion.Operation.Should().Be(Operation.None);
        criterion.Arguments.Should().BeEmpty();
    }

    [Test]
    public void New_SimpleOverloads_ShouldHaveExpected()
    {
        var criterion = new Criterion(Element.Tag.Property("Value"), Operation.Between, 0, 100);

        criterion.Type.Should().Be(typeof(Tag));
        criterion.Property.Should().Be(Element.Tag.Property("Value"));
        criterion.Operation.Should().Be(Operation.Between);
        criterion.Arguments.Should().HaveCount(2);
    }

    [Test]
    public void New_InnerCriterionArgument_ShouldHaveExpected()
    {
        var criterion = new Criterion(Element.Tag.Property("Members"), Operation.Any,
            new Criterion(Element.Tag.Property("Name"), Operation.Containing, "Test"));

        criterion.Type.Should().Be(typeof(Tag));
        criterion.Property.Should().Be(Element.Tag.Property("Members"));
        criterion.Operation.Should().Be(Operation.Any);
        criterion.Arguments.Should().HaveCount(1);
        criterion.Arguments.First().Value.Should().BeOfType<Criterion>();
    }

    [Test]
    public void Evaluate_SimpleImmediateProperty_ShouldBeExpectedResult()
    {
        var tag = new Tag { Name = "Test" };

        var criterion = new Criterion(Element.Tag.Property("Name"), Operation.EqualTo, "Test");

        var evaluation = criterion.Evaluate(tag);

        evaluation.Result.Should().Be(ResultState.Passed);
    }

    [Test]
    public void Evaluate_StringPropertyWithNumericArguments_ShouldHaveFailedButNotErrorBecauseTheArgumentsAreConverted()
    {
        var tag = new Tag { Name = "Test" };

        var criterion = new Criterion(Element.Tag.Property("Name"), Operation.Between, 1, 10);

        var evaluation = criterion.Evaluate(tag);

        evaluation.Result.Should().Be(ResultState.Failed);
    }

    [Test]
    public void Evaluate_StringBetweenValidValues_ShouldHavePassedEvaluation()
    {
        var tag = new Tag { Name = "MyName" };

        var criterion = new Criterion(Element.Tag.Property("Name"), Operation.Between, "C", "T");

        var evaluation = criterion.Evaluate(tag);

        evaluation.Result.Should().Be(ResultState.Passed);
    }

    [Test]
    public void AsExpression_WhenCompiled_ShouldAlsoWork()
    {
        var tag = new Tag { Name = "Test" };

        var criterion = new Criterion(Element.Tag.Property("Name"), Operation.EqualTo, "Test");


        var expression = (Expression<Func<object?, bool>>)criterion;
        var func = expression.Compile();

        var result = func(tag);
        result.Should().BeTrue();
    }

    [Test]
    public void VariableArgument_WhenEvaluated_ShouldRunCorrectly()
    {
        var tag = new Tag { Name = "MyTestTag" };
        var variable = new Variable("MyVar", "Test");
        var criterion = new Criterion(Element.Tag.Property("Name"), Operation.Containing, variable);

        var eval = criterion.Evaluate(tag);

        eval.Result.Should().Be(ResultState.Passed);
    }

    [Test]
    public void VariableArgument_EnumStringValue_ShouldRunCorrectly()
    {
        var tag = new Tag { Name = "MyTestTag", Value = 123 };
        var variable = new Variable("MyVar", "Decimal");
        var criterion = new Criterion(Element.Tag.Property("Radix"), Operation.EqualTo, variable);

        var eval = criterion.Evaluate(tag);

        eval.Result.Should().Be(ResultState.Passed);
    }

    [Test]
    public void VariableArgument_LogixTypeStringValue_ShouldRunCorrectly()
    {
        var tag = new Tag { Name = "TestTag", Value = 1.21f };
        var variable = new Variable("Value", "1.221");
        var criterion = new Criterion(Element.Tag.Property("Value"), Operation.GreaterThan, variable);

        var eval = criterion.Evaluate(tag);

        eval.Result.Should().Be(ResultState.Failed);
    }

    [Test]
    public void VariableArgument_LogixTypeFloatEquals_ShouldRunCorrectly()
    {
        var tag = new Tag { Name = "TestTag", Value = 1.2345f };
        var variable = new Variable("Value", new REAL(1.2345f));
        var criterion = new Criterion(Element.Tag.Property("Value"), Operation.EqualTo, variable);

        var eval = criterion.Evaluate(tag);

        eval.Result.Should().Be(ResultState.Passed);
    }

    [Test]
    public void ToString_SimpleStringArgument_ShouldBeExpected()
    {
        var criterion = new Criterion(Element.Tag.Property("Name"), Operation.EqualTo, "Test");

        var result = criterion.ToString();

        result.Should().Be("Name Equal Test");
    }

    [Test]
    public void ToString_SimpleEnumArgument_ShouldBeExpected()
    {
        var criterion = new Criterion(Element.Tag.Property("Radix"), Operation.EqualTo, Radix.Ascii);

        var result = criterion.ToString();

        result.Should().Be("Radix Equal Ascii");
    }

    [Test]
    public void ToString_SimpleUnaryOperation_ShouldBeExpected()
    {
        var criterion = new Criterion(Element.Tag.Property("Constant"), Operation.False);

        var result = criterion.ToString();

        result.Should().Be("Constant Is False");
    }

    [Test]
    public void ToString_NestedCriterion_ShouldBeExpected()
    {
        var criterion = new Criterion(Element.Tag.Property("Members"), Operation.Any,
            new Criterion(Element.Tag.Property("TagName"), Operation.Containing, "SomeValue"));

        var result = criterion.ToString();

        result.Should().Be("Members Any TagName Contains SomeValue");
    }

    [Test]
    public void ToString_NestedVariable_ShouldBeExpected()
    {
        var criterion = new Criterion(Element.Tag.Property("Name"), Operation.Like, new Variable("Test", "%Test_%"));

        var result = criterion.ToString();

        result.Should().Be("Name Like %Test_%");
    }
}