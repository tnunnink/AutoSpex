using System.Linq.Expressions;
using System.Text.Json;
using Task = System.Threading.Tasks.Task;

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
        criterion.Argument.Should().BeNull();
    }

    [Test]
    public void New_SimpleOverloads_ShouldHaveExpected()
    {
        var criterion = new Criterion(Element.Tag.Property("Value"), Operation.Between, 0, 100);

        criterion.Type.Should().Be(typeof(Tag));
        criterion.Property.Should().Be(Element.Tag.Property("Value"));
        criterion.Operation.Should().Be(Operation.Between);
        criterion.Argument.Should().BeOfType<List<object>>();
        criterion.Argument.As<List<object>>().Should().HaveCount(2);
    }

    [Test]
    public void New_InnerCriterionArgument_ShouldHaveExpected()
    {
        var innerCriterion = new Criterion(Element.Tag.Property("Name"), Operation.Containing, "Test");
        var criterion = new Criterion(Element.Tag.Property("Members"), Operation.Any, innerCriterion);

        criterion.Type.Should().Be(typeof(Tag));
        criterion.Property.Should().Be(Element.Tag.Property("Members"));
        criterion.Operation.Should().Be(Operation.Any);
        criterion.Argument.Should().BeOfType<Criterion>();
        criterion.Argument.Should().BeEquivalentTo(innerCriterion);
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
        var criterion = new Criterion(Element.Tag.Property("Constant"), Operation.Null);

        var result = criterion.ToString();

        result.Should().Be("Constant Is Null");
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
    public void Parse_ValidExampleUnaryOperationOnTag_ShouldBeExpected()
    {
        var type = typeof(Tag);
        const string text = "TagName Is Null";

        var criterion = Criterion.Parse(type, text);

        criterion.Should().NotBeNull();
        criterion.Type.Should().Be(typeof(Tag));
        criterion.Property.Should().Be(Property.This(type).GetProperty("TagName"));
        criterion.Negation.Should().Be(Negation.Is);
        criterion.Operation.Should().Be(Operation.Null);
        criterion.Argument.Should().BeNull();
    }

    [Test]
    public void Parse_ValidExampleBinaryOperationOnTag_ShouldBeExpected()
    {
        var type = typeof(Tag);
        const string text = "Value Is Equal To 123";

        var criterion = Criterion.Parse(type, text);

        criterion.Should().NotBeNull();
        criterion.Type.Should().Be(typeof(Tag));
        criterion.Property.Should().Be(Property.This(type).GetProperty("Value"));
        criterion.Negation.Should().Be(Negation.Is);
        criterion.Operation.Should().Be(Operation.EqualTo);
        criterion.Argument.Should().Be(123);
    }

    [Test]
    public void Parse_ValidExampleTernaryOperationOnTag_ShouldBeExpected()
    {
        var type = typeof(Tag);
        const string text = "Value Is Between 1 and 10";

        var criterion = Criterion.Parse(type, text);

        criterion.Should().NotBeNull();
        criterion.Type.Should().Be(typeof(Tag));
        criterion.Property.Should().Be(Property.This(type).GetProperty("Value"));
        criterion.Negation.Should().Be(Negation.Is);
        criterion.Operation.Should().Be(Operation.Between);
        criterion.Argument.Should().BeEquivalentTo(new Range(1, 10));
    }

    [Test]
    public void Parse_ValidExampleCollectionOperationOnTag_ShouldBeExpected()
    {
        var type = typeof(Tag);
        const string text = "Members Is Any TagName Is Containing This is some text";

        var criterion = Criterion.Parse(type, text);

        criterion.Should().NotBeNull();
        criterion.Type.Should().Be(typeof(Tag));
        criterion.Property.Should().Be(Property.This(type).GetProperty("Members"));
        criterion.Negation.Should().Be(Negation.Is);
        criterion.Operation.Should().Be(Operation.Any);
        criterion.Argument.Should().BeOfType<Criterion>();
        criterion.Argument.As<Criterion>().Property.Should().Be(Property.This(typeof(Tag)).GetProperty("TagName"));
        criterion.Argument.As<Criterion>().Negation.Should().Be(Negation.Is);
        criterion.Argument.As<Criterion>().Operation.Should().Be(Operation.Containing);
        criterion.Argument.As<Criterion>().Argument.Should().Be("This is some text");
    }

    [Test]
    public void Parse_ValidExampleInOperationOnTagEnum_ShouldBeExpected()
    {
        var type = typeof(Tag);
        const string text = "Radix Not In [Decimal,Octal,Binary]";

        var criterion = Criterion.Parse(type, text);

        criterion.Should().NotBeNull();
        criterion.Type.Should().Be(typeof(Tag));
        criterion.Property.Should().Be(Property.This(type).GetProperty("Radix"));
        criterion.Negation.Should().Be(Negation.Not);
        criterion.Operation.Should().Be(Operation.In);
        criterion.Argument.Should().BeOfType<List<object>>();
        criterion.Argument.As<List<object>>().Should().ContainInOrder([Radix.Decimal, Radix.Octal, Radix.Binary]);
    }

    [Test]
    public void Parse_InvalidPropertyName_ShouldWorkButNotParseValue()
    {
        var type = typeof(Tag);
        const string text = "WTF Not Equal To Something";

        var criterion = Criterion.Parse(type, text);

        criterion.Should().NotBeNull();
        criterion.Type.Should().Be(typeof(Tag));
        criterion.Property.Should().Be(Property.This(type).GetProperty("WTF"));
        criterion.Negation.Should().Be(Negation.Not);
        criterion.Operation.Should().Be(Operation.EqualTo);
        criterion.Argument.Should().Be("Something");
    }

    [Test]
    public void Parse_InvalidNegation_ShouldThrowException()
    {
        var type = typeof(Tag);
        const string text = "TagName IsNot Equal To Something";

        FluentActions.Invoking(() => Criterion.Parse(type, text)).Should().Throw<FormatException>();
    }

    [Test]
    public void Parse_InvalidOperation_ShouldThrowException()
    {
        var type = typeof(Tag);
        const string text = "TagName Is Whatever Something";

        FluentActions.Invoking(() => Criterion.Parse(type, text)).Should().Throw<FormatException>();
    }

    [Test]
    public Task Serialize_ValidCriterion_ShouldBeVerified()
    {
        var criterion = new Criterion(Element.Tag.Property("Name"), Operation.EqualTo, "Testing");

        var json = JsonSerializer.Serialize(criterion);

        return VerifyJson(json);
    }

    [Test]
    public Task Serialize_ConfiguredSpecWithRange_ShouldBeVerified()
    {
        var spec = new Spec();

        spec.Query(Element.Tag)
            .Filter("Name", Operation.Containing, "Test")
            .Verify("Value", Negation.Is, Operation.Between, new Range(1, 10));

        return VerifyJson(JsonSerializer.Serialize(spec));
    }

    [Test]
    public void Deserialize_WhenCalled_ShouldBeExpected()
    {
        var spec = new Spec();
        spec.Query(Element.Tag)
            .Filter("Name", Operation.Containing, "Test")
            .Verify("DataType", Negation.Not, Operation.NullOrEmpty);
        var data = JsonSerializer.Serialize(spec);

        var result = JsonSerializer.Deserialize<Spec>(data);

        result?.Element.Should().Be(Element.Tag);
        result?.Filters.Should().HaveCount(1);
        result?.Verifications.Should().HaveCount(1);

        result.Should().BeEquivalentTo(spec);
    }
}