// ReSharper disable UseObjectOrCollectionInitializer

using System.Text.Json;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Engine.Tests;

[TestFixture]
public class VariableTests
{
    [Test]
    public void New_Default_ShouldBeExpected()
    {
        var variable = new Variable();

        variable.VariableId.Should().NotBeEmpty();
        variable.Name.Should().BeEmpty();
        variable.Group.Should().Be(TypeGroup.Text);
        variable.Value.Should().BeNull();
    }

    [Test]
    public void New_TypeGroupBoolean_ShouldHaveExpectedValues()
    {
        var variable = new Variable(TypeGroup.Boolean);

        variable.VariableId.Should().NotBeEmpty();
        variable.Group.Should().Be(TypeGroup.Boolean);
    }

    [Test]
    public void New_TypeGroupNumber_ShouldHaveExpectedValues()
    {
        var variable = new Variable(TypeGroup.Number);

        variable.VariableId.Should().NotBeEmpty();
        variable.Group.Should().Be(TypeGroup.Number);
    }

    [Test]
    public void New_TypeGroupText_ShouldHaveExpectedValues()
    {
        var variable = new Variable(TypeGroup.Text);

        variable.VariableId.Should().NotBeEmpty();
        variable.Group.Should().Be(TypeGroup.Text);
    }

    [Test]
    public void New_TypeGroupDate_ShouldHaveExpectedValues()
    {
        var variable = new Variable(TypeGroup.Date);

        variable.VariableId.Should().NotBeEmpty();
        variable.Group.Should().Be(TypeGroup.Date);
    }

    [Test]
    public void New_TypeGroupEnum_ShouldHaveExpectedValues()
    {
        var variable = new Variable(TypeGroup.Enum);

        variable.VariableId.Should().NotBeEmpty();
        variable.Group.Should().Be(TypeGroup.Enum);
    }

    [Test]
    public void New_TypeGroupElement_ShouldHaveExpectedValues()
    {
        var variable = new Variable(TypeGroup.Element);

        variable.VariableId.Should().NotBeEmpty();
        variable.Group.Should().Be(TypeGroup.Element);
    }

    [Test]
    public void New_TypeGroupCollection_ShouldHaveExpectedValues()
    {
        var variable = new Variable(TypeGroup.Collection);

        variable.VariableId.Should().NotBeEmpty();
        variable.Group.Should().Be(TypeGroup.Collection);
    }

    [Test]
    public void New_NameAndValueOverload_ShouldHaveExpectedValues()
    {
        var variable = new Variable("MyVar", "SomeValue");

        variable.VariableId.Should().NotBeEmpty();
        variable.Name.Should().Be("MyVar");
        variable.Group.Should().Be(TypeGroup.Text);
        variable.Value.Should().Be("SomeValue");
    }

    [Test]
    public void Value_SetToNull_ShouldAllBeNull()
    {
        var variable = new Variable("MyVar", "TestValue");

        variable.Value = null;

        variable.Value.Should().BeNull();
    }

    [Test]
    public void Value_SetToText_ShouldBeExpected()
    {
        var variable = new Variable();

        variable.Value = "Something";

        variable.Value.Should().Be("Something");
    }

    [Test]
    public void Value_SetToInteger_ShouldBeExpected()
    {
        var variable = new Variable();

        variable.Value = 123;

        variable.Value.Should().Be(123);
    }

    [Test]
    public void Value_SetToDouble_ShouldBeExpected()
    {
        var variable = new Variable();

        variable.Value = 1.23;

        variable.Value.Should().Be(1.23);
    }

    [Test]
    public void Value_SetToBoolean_ShouldBeExpected()
    {
        var variable = new Variable();

        variable.Value = true;

        variable.Value.Should().Be(true);
    }

    [Test]
    public void Value_SetToDint_ShouldBeExpected()
    {
        var variable = new Variable();

        variable.Value = new DINT(123);

        variable.Value.Should().Be(123);
    }

    [Test]
    public void Value_SetToReal_ShouldBeExpected()
    {
        var variable = new Variable();

        variable.Value = new REAL(1.23f);

        variable.Value.Should().Be(1.23f);
    }

    [Test]
    public void Value_SetToBool_ShouldBeExpected()
    {
        var variable = new Variable();

        variable.Value = new BOOL(true);

        variable.Value.Should().Be(true);
    }

    [Test]
    public void Value_SetToLogixEnum_ShouldBeExpected()
    {
        var variable = new Variable();

        variable.Value = ExternalAccess.None;

        variable.Value.Should().Be(ExternalAccess.None);
    }

    [Test]
    public void Value_SetToRadix_ShouldBeExpected()
    {
        var variable = new Variable();

        variable.Value = Radix.Ascii;

        variable.Value.Should().Be(Radix.Ascii);
    }

    [Test]
    public void Value_SetToTag_ShouldBeExpected()
    {
        var variable = new Variable();

        var tag = new Tag { Name = "Test", Value = new TIMER() };

        variable.Value = tag;

        variable.Value.Should().BeEquivalentTo(tag);
    }

    [Test]
    public Task Serialize_SimpleValue_ShouldBeVerified()
    {
        var variable = new Variable("Test", 123);

        var data = JsonSerializer.Serialize(variable);

        return VerifyJson(data);
    }

    [Test]
    public void Deserialize_SimpleValue_ShouldBeExpected()
    {
        var variable = new Variable("Test", 123);
        var data = JsonSerializer.Serialize(variable);

        var result = JsonSerializer.Deserialize<Variable>(data);

        result.Should().BeEquivalentTo(variable);
    }
}