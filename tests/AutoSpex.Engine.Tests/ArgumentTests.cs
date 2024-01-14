namespace AutoSpex.Engine.Tests;

[TestFixture]
public class ArgumentTests
{
    [Test]
    public void New_StringType_ShouldHaveExpectedValues()
    {
        var argument = new Argument("Test");

        argument.Should().NotBeNull();
        argument.Value.Should().Be("Test");
        argument.Type.Should().Be(typeof(string));
        argument.Group.Should().Be(TypeGroup.Text);
    }

    [Test]
    public void New_BooleanType_ShouldHaveExpectedValues()
    {
        var argument = new Argument(true);

        argument.Should().NotBeNull();
        argument.Value.Should().Be(true);
        argument.Type.Should().Be(typeof(bool));
        argument.Group.Should().Be(TypeGroup.Boolean);
    }

    [Test]
    public void New_NumberType_ShouldHaveExpectedValues()
    {
        var argument = new Argument(123);

        argument.Should().NotBeNull();
        argument.Value.Should().Be(123);
        argument.Type.Should().Be(typeof(int));
        argument.Group.Should().Be(TypeGroup.Number);
    }
    
    [Test]
    public void New_EnumType_ShouldHaveExpectedValues()
    {
        var argument = new Argument(Radix.Decimal);

        argument.Should().NotBeNull();
        argument.Value.Should().Be(Radix.Decimal);
        argument.Type.Should().Be(Radix.Decimal.GetType());
        argument.Group.Should().Be(TypeGroup.Enum);
    }

    [Test]
    public void New_VariableType_ShouldHaveExpectedValues()
    {
        var value = new Variable("Test", "Test");
        var argument = new Argument(value);

        argument.Should().NotBeNull();
        argument.Value.Should().BeEquivalentTo(value);
        argument.Type.Should().Be(typeof(Variable));
        argument.Group.Should().Be(TypeGroup.Default);
    }
    
    [Test]
    public void New_CriterionType_ShouldHaveExpectedValues()
    {
        var value = new Criterion("Test", Operation.Contains, "Something");
        var argument = new Argument(value);

        argument.Should().NotBeNull();
        argument.Value.Should().BeEquivalentTo(value);
        argument.Type.Should().Be(typeof(Criterion));
        argument.Group.Should().Be(TypeGroup.Default);
    }
    
    [Test]
    public void Resolve_StringTypeAsString_ShouldHaveExpectedValues()
    {
        var argument = new Argument("Test");

        var result = argument.Resolve(typeof(string));

        result.Should().Be("Test");
        result.Should().BeOfType<string>();
    }
    
    [Test]
    public void Resolve_BoolAsBool_ShouldHaveExpectedValues()
    {
        var argument = new Argument(true);

        var result = argument.Resolve(typeof(bool));

        result.Should().Be(true);
        result.Should().BeOfType<bool>();
    }
    
    [Test]
    public void Resolve_BoolAsBoolToNullableBool_ShouldStillBeJustBool()
    {
        var argument = new Argument(true);

        var result = argument.Resolve(typeof(bool?));

        result.Should().Be(true);
        result.Should().BeOfType<bool>();
    }
    
    [Test]
    public void Resolve_BoolAsStringToNullableBool_ShouldStillBeJustBool()
    {
        var argument = new Argument("true");

        var result = argument.Resolve(typeof(bool?));

        result.Should().Be(true);
        result.Should().BeOfType<bool>();
    }

    [Test]
    public void Resolve_IntAsStringToInt_ShouldBeExpectedValue()
    {
        var argument = new Argument("123");

        var result = argument.Resolve(typeof(int));

        result.Should().Be(123);
        result.Should().BeOfType<int>();
    }
    
    [Test]
    public void Resolve_FloatAsStringToFloat_ShouldBeExpectedValue()
    {
        var argument = new Argument("12.312345");

        var result = argument.Resolve(typeof(float));

        result.Should().Be(12.312345f);
        result.Should().BeOfType<float>();
    }
    
    [Test]
    public void Resolve_DateTimeAsStringToDateTime_ShouldBeExpectedValue()
    {
        var argument = new Argument("12/15/2023 10:34:22");

        var result = argument.Resolve(typeof(DateTime));

        result.Should().Be(new DateTime(2023, 12, 15, 10, 34, 22));
        result.Should().BeOfType<DateTime>();
    }
    
    [Test]
    public void Resolve_CriterionRegardlessOfType_ShouldHaveExpectedValues()
    {
        var value = new Criterion("Test", Operation.Contains, "Something");
        var argument = new Argument(value);

        var result = argument.Resolve(typeof(string));

        result.Should().BeEquivalentTo(value);
        result.Should().BeOfType<Criterion>();
    }
    
    [Test]
    public void Resolve_VariableAaString_ShouldHaveExpectedValues()
    {
        var value = new Variable("Test", "Test");
        var argument = new Argument(value);

        var result = argument.Resolve(typeof(string));

        result.Should().Be("Test");
        result.Should().BeOfType<string>();
    }
}