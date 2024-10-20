﻿using System.Linq.Expressions;

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
        criterion.Argument.Should().NotBeNull();
        criterion.Argument.Value.Should().BeNull();
    }

    [Test]
    public void New_SimpleOverloads_ShouldHaveExpected()
    {
        var criterion = new Criterion(Element.Tag.Property("Value"), Operation.Between, 0, 100);

        criterion.Type.Should().Be(typeof(Tag));
        criterion.Property.Should().Be(Element.Tag.Property("Value"));
        criterion.Operation.Should().Be(Operation.Between);
        criterion.Argument.Value.Should().BeOfType<List<Argument>>();
        criterion.Argument.Value.As<List<Argument>>().Should().HaveCount(2);
    }

    [Test]
    public void New_InnerCriterionArgument_ShouldHaveExpected()
    {
        var innerCriterion = new Criterion(Element.Tag.Property("Name"), Operation.Containing, "Test");
        var criterion = new Criterion(Element.Tag.Property("Members"), Operation.Any, innerCriterion);

        criterion.Type.Should().Be(typeof(Tag));
        criterion.Property.Should().Be(Element.Tag.Property("Members"));
        criterion.Operation.Should().Be(Operation.Any);
        criterion.Argument.Value.Should().BeOfType<Criterion>();
        criterion.Argument.Value.Should().BeEquivalentTo(innerCriterion);
        
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

        var expression = (Expression<Func<object, bool>>)criterion;
        var func = expression.Compile();

        var result = func(tag);
        result.Should().BeTrue();
    }

    [Test]
    public void VariableArgument_WhenEvaluated_ShouldRunCorrectly()
    {
        var tag = new Tag { Name = "MyTestTag" };
        var variable = new Variable("MyVar", "Test");
        var criterion = new Criterion(Element.Tag.Property("Name"), Operation.Containing, variable.Reference());

        var eval = criterion.Evaluate(tag);

        eval.Result.Should().Be(ResultState.Passed);
    }

    [Test]
    public void VariableArgument_EnumStringValue_ShouldRunCorrectly()
    {
        var tag = new Tag { Name = "MyTestTag", Value = 123 };
        var variable = new Variable("MyVar", "Decimal");
        var criterion = new Criterion(Element.Tag.Property("Radix"), Operation.EqualTo, variable.Reference());

        var eval = criterion.Evaluate(tag);

        eval.Result.Should().Be(ResultState.Passed);
    }

    [Test]
    public void VariableArgument_LogixTypeStringValue_ShouldRunCorrectly()
    {
        var tag = new Tag { Name = "TestTag", Value = 1.21f };
        var variable = new Variable("Value", "1.221");
        var criterion = new Criterion(Element.Tag.Property("Value"), Operation.GreaterThan, variable.Reference());

        var eval = criterion.Evaluate(tag);

        eval.Result.Should().Be(ResultState.Failed);
    }

    [Test]
    public void VariableArgument_LogixTypeFloatEquals_ShouldRunCorrectly()
    {
        var tag = new Tag { Name = "TestTag", Value = 1.2345f };
        var variable = new Variable("Value", new REAL(1.2345f));
        var criterion = new Criterion(Element.Tag.Property("Value"), Operation.EqualTo, variable.Reference());

        var eval = criterion.Evaluate(tag);

        eval.Result.Should().Be(ResultState.Passed);
    }

    [Test]
    public void ToString_SimpleStringArgument_ShouldBeExpected()
    {
        var criterion = new Criterion(Element.Tag.Property("Name"), Operation.EqualTo, "Test");

        var result = criterion.ToString();

        result.Should().Be("Name Is Equal To Test");
    }

    [Test]
    public void ToString_SimpleEnumArgument_ShouldBeExpected()
    {
        var criterion = new Criterion(Element.Tag.Property("Radix"), Operation.EqualTo, Radix.Ascii);

        var result = criterion.ToString();

        result.Should().Be("Radix Is Equal To Ascii");
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

        result.Should().Be("Members Is Any TagName Is Containing SomeValue");
    }

    [Test]
    public void ToString_NestedVariable_ShouldBeExpected()
    {
        var criterion = new Criterion(Element.Tag.Property("Name"), Operation.Like,
            new Variable("Test", "%Test_%").Reference());

        var result = criterion.ToString();

        result.Should().Be("Name Is Like %Test_%");
    }

    [Test]
    public void True_WhenEvaluatedWithSomeString_ShouldBePassed()
    {
        var criterion = Criterion.True;

        var evaluation = criterion.Evaluate("This should not matter");

        evaluation.Result.Should().Be(ResultState.Passed);
    }

    [Test]
    public void True_WhenEvaluatedWithFalse_ShouldBePassed()
    {
        var criterion = Criterion.True;

        var evaluation = criterion.Evaluate(false);

        evaluation.Result.Should().Be(ResultState.Passed);
    }

    [Test]
    public void True_WhenEvaluatedWithNull_ShouldBePassed()
    {
        var criterion = Criterion.True;

        var evaluation = criterion.Evaluate(null!);

        evaluation.Result.Should().Be(ResultState.Passed);
    }

    [Test]
    public void False_WhenEvaluatedWithSomeString_ShouldBeFailed()
    {
        var criterion = Criterion.False;

        var evaluation = criterion.Evaluate("This should not matter");

        evaluation.Result.Should().Be(ResultState.Failed);
    }

    [Test]
    public void False_WhenEvaluatedWithFalse_ShouldBeFailed()
    {
        var criterion = Criterion.False;

        var evaluation = criterion.Evaluate(false);

        evaluation.Result.Should().Be(ResultState.Failed);
    }

    [Test]
    public void False_WhenEvaluatedWithNull_ShouldBeFailed()
    {
        var criterion = Criterion.False;

        var evaluation = criterion.Evaluate(null!);

        evaluation.Result.Should().Be(ResultState.Failed);
    }
}