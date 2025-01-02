using System.Text.Json;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Engine.Tests;

[TestFixture]
public class CriterionTests
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };


    [Test]
    public void New_Default_ShouldHaveExpectedDefaults()
    {
        var criterion = new Criterion();

        criterion.Should().NotBeNull();
        criterion.Property.Should().Be(string.Empty);
        criterion.Operation.Should().Be(Operation.None);
        criterion.Argument.Should().BeNull();
    }

    [Test]
    public void New_SimpleOverloads_ShouldHaveExpected()
    {
        var criterion = new Criterion("Value", Operation.Between, new Range(0, 100));

        criterion.Property.Should().Be("Value");
        criterion.Operation.Should().Be(Operation.Between);
        criterion.Argument.Should().BeOfType<Range>();
        criterion.Argument.Should().BeEquivalentTo(new Range(0, 100));
    }

    [Test]
    public void New_InnerCriterionArgument_ShouldHaveExpected()
    {
        var inner = new Criterion("Name", Operation.Containing, "Test");
        var criterion = new Criterion("Members", Operation.Any, inner);

        criterion.Property.Should().Be("Members");
        criterion.Operation.Should().Be(Operation.Any);
        criterion.Argument.Should().BeOfType<Criterion>();
        criterion.Argument.Should().BeEquivalentTo(inner);
    }

    [Test]
    public void Evaluate_UnaryOperation_ShouldBeExpectedResult()
    {
        var tag = new Tag { Name = "Test" };
        var criterion = new Criterion("Name", Operation.Void) { Negation = Negation.Not };

        var evaluation = criterion.Evaluate(tag);

        evaluation.Result.Should().Be(ResultState.Passed);
    }

    [Test]
    public void Evaluate_BinaryOperation_ShouldBeExpectedResult()
    {
        var tag = new Tag { Name = "Test" };
        var criterion = new Criterion("Name", Operation.EqualTo, "Test");

        var evaluation = criterion.Evaluate(tag);

        evaluation.Result.Should().Be(ResultState.Passed);
    }

    [Test]
    public void Evaluate_BetweenOperation_ShouldBeExpectedResult()
    {
        var tag = new Tag { Name = "MyName", Value = 12 };
        var criterion = new Criterion("Value", Operation.Between, new Range(1, 10));

        var evaluation = criterion.Evaluate(tag);

        evaluation.Result.Should().Be(ResultState.Failed);
    }

    [Test]
    public void Evaluate_InOperation_ShouldBeExpectedResult()
    {
        var tag = new Tag { Name = "MyName", Value = 12 };
        var criterion = new Criterion("Value", Operation.In, new List<int> { 1, 10, 12 });

        var evaluation = criterion.Evaluate(tag);

        evaluation.Result.Should().Be(ResultState.Passed);
    }

    [Test]
    public void Evaluate_CollectionOperation_ShouldBeExpectedResult()
    {
        var tag = new Tag { Name = "MyName", Value = new TIMER { PRE = 1230 } };
        var criterion = new Criterion("Members", Operation.Any, new Criterion("Value", Operation.EqualTo, 1230));

        var evaluation = criterion.Evaluate(tag);

        evaluation.Result.Should().Be(ResultState.Passed);
    }

    [Test]
    public void ToString_SimpleStringArgument_ShouldBeExpected()
    {
        var criterion = new Criterion("Name", Operation.EqualTo, "Test");

        var result = criterion.ToString();

        result.Should().Be("Name Is Equal To Test");
    }

    [Test]
    public void ToString_SimpleEnumArgument_ShouldBeExpected()
    {
        var criterion = new Criterion("Radix", Operation.EqualTo, Radix.Ascii);

        var result = criterion.ToString();

        result.Should().Be("Radix Is Equal To Ascii");
    }

    [Test]
    public void ToString_SimpleUnaryOperation_ShouldBeExpected()
    {
        var criterion = new Criterion("Constant", Operation.Null);

        var result = criterion.ToString();

        result.Should().Be("Constant Is Null");
    }

    [Test]
    public void ToString_NestedCriterion_ShouldBeExpected()
    {
        var criterion = new Criterion("Members", Operation.Any,
            new Criterion("TagName", Operation.Containing, "SomeValue"));

        var result = criterion.ToString();

        result.Should().Be("Members Is Any TagName Is Containing SomeValue");
    }

    [Test]
    public Task Serialize_DefaultCriterion_ShouldBeVerified()
    {
        var criterion = new Criterion();

        var json = JsonSerializer.Serialize(criterion, Options);

        return Verify(json);
    }

    [Test]
    public Task Serialize_ValidCriterion_ShouldBeVerified()
    {
        var criterion = new Criterion("Name", Operation.EqualTo, "Testing");

        var json = JsonSerializer.Serialize(criterion, Options);

        return Verify(json);
    }

    [Test]
    public Task Serialize_WithNegation_ShouldBeVerified()
    {
        var criterion = new Criterion("Name", Operation.EqualTo, "Testing") { Negation = Negation.Not };

        var json = JsonSerializer.Serialize(criterion, Options);

        return Verify(json);
    }

    [Test]
    public Task Serialize_RangeArgument_ShouldBeVerified()
    {
        var criterion = new Criterion("Value", Operation.Between, new Range(1, 10));

        var json = JsonSerializer.Serialize(criterion, Options);

        return Verify(json);
    }

    [Test]
    public Task Serialize_CollectionArgument_ShouldBeVerified()
    {
        var criterion = new Criterion("Radix", Operation.In,
            new List<object> { Radix.Decimal, Radix.Float, Radix.Binary });

        var json = JsonSerializer.Serialize(criterion, Options);

        return Verify(json);
    }

    [Test]
    public Task Serialize_CriterionArgument_ShouldBeVerified()
    {
        var criterion = new Criterion("Members", Operation.Any,
            new Criterion("TagName", Operation.Containing, "Testing"));

        var json = JsonSerializer.Serialize(criterion, Options);

        return Verify(json);
    }

    [Test]
    public void Deserialize_WhenCalled_ShouldBeExpected()
    {
        var criterion = new Criterion("Name", Operation.EqualTo, "Testing");
        var json = JsonSerializer.Serialize(criterion);

        var result = JsonSerializer.Deserialize<Criterion>(json);

        result.Should().BeEquivalentTo(criterion);
    }
}