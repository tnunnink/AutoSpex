﻿// ReSharper disable UseObjectOrCollectionInitializer

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
        variable.Path.Should().BeEmpty();
        variable.ScopedReference.Should().Be("{}");
        variable.AbsoluteReference.Should().Be("{}");
        variable.Group.Should().Be(TypeGroup.Text);
        variable.Type.Should().Be(typeof(string));
        variable.Value.Should().Be(string.Empty);
        variable.Data.Should().Be(string.Empty);
        variable.Description.Should().BeNull();
    }

    [Test]
    public void New_TypeGroupBoolean_ShouldHaveExpectedValues()
    {
        var variable = new Variable(TypeGroup.Boolean);

        variable.VariableId.Should().NotBeEmpty();
        variable.Type.Should().Be(typeof(bool));
        variable.Group.Should().Be(TypeGroup.Boolean);
        variable.Value.Should().Be(false);
        variable.Data.Should().Be("False");
    }

    [Test]
    public void New_TypeGroupNumber_ShouldHaveExpectedValues()
    {
        var variable = new Variable(TypeGroup.Number);

        variable.VariableId.Should().NotBeEmpty();
        variable.Type.Should().Be(typeof(int));
        variable.Group.Should().Be(TypeGroup.Number);
        variable.Value.Should().Be(0);
        variable.Data.Should().Be("0");
    }

    [Test]
    public void New_TypeGroupText_ShouldHaveExpectedValues()
    {
        var variable = new Variable(TypeGroup.Text);

        variable.VariableId.Should().NotBeEmpty();
        variable.Type.Should().Be(typeof(string));
        variable.Group.Should().Be(TypeGroup.Text);
        variable.Value.Should().Be("");
        variable.Data.Should().Be("");
    }

    [Test]
    public void New_TypeGroupDate_ShouldHaveExpectedValues()
    {
        var variable = new Variable(TypeGroup.Date);

        variable.VariableId.Should().NotBeEmpty();
        variable.Type.Should().Be(typeof(DateTime));
        variable.Group.Should().Be(TypeGroup.Date);
        variable.Value.Should().Be(DateTime.Today);
        // ReSharper disable once SpecifyACultureInStringConversionExplicitly
        variable.Data.Should().Be(DateTime.Today.ToString());
    }

    [Test]
    public void New_TypeGroupEnum_ShouldHaveExpectedValues()
    {
        var variable = new Variable(TypeGroup.Enum);

        variable.VariableId.Should().NotBeEmpty();
        variable.Group.Should().Be(TypeGroup.Enum);
        variable.Type.Should().BeNull();
        variable.Value.Should().BeNull();
        variable.Data.Should().BeNull();
    }

    [Test]
    public void New_TypeGroupElement_ShouldHaveExpectedValues()
    {
        var variable = new Variable(TypeGroup.Element);

        variable.VariableId.Should().NotBeEmpty();
        variable.Group.Should().Be(TypeGroup.Element);
        variable.Type.Should().BeNull();
        variable.Value.Should().BeNull();
        variable.Data.Should().BeNull();
    }

    [Test]
    public void New_TypeGroupCollection_ShouldHaveExpectedValues()
    {
        var variable = new Variable(TypeGroup.Collection);

        variable.VariableId.Should().NotBeEmpty();
        variable.Group.Should().Be(TypeGroup.Collection);
        variable.Type.Should().Be(typeof(List<object>));
        //todo we need to work out collection. How is it serialized and parsed?
        /*variable.Value.Should().Be(new List<object>());
        variable.Data.Should().NotBeEmpty();*/
    }

    [Test]
    public void New_NameAndValueOverload_ShouldHaveExpectedValues()
    {
        var variable = new Variable("MyVar", "SomeValue");

        variable.VariableId.Should().NotBeEmpty();
        variable.Name.Should().Be("MyVar");
        variable.ScopedReference.Should().Be("{MyVar}");
        variable.AbsoluteReference.Should().Be("{MyVar}");
        variable.Type.Should().Be(typeof(string));
        variable.Group.Should().Be(TypeGroup.Text);
        variable.Value.Should().Be("SomeValue");
        variable.Data.Should().Be("SomeValue");
    }

    [Test]
    public void Value_SetToNull_ShouldAllBeNull()
    {
        var variable = new Variable("MyVar", "TestValue");

        variable.Value = null;

        variable.Value.Should().BeNull();
        variable.Data.Should().BeNull();
        variable.Type.Should().BeNull();
    }

    [Test]
    public void Value_SetToText_ShouldBeExpected()
    {
        var variable = new Variable();

        variable.Value = "Something";

        variable.Value.Should().Be("Something");
        variable.Type.Should().Be(typeof(string));
        variable.Data.Should().Be("Something");
    }

    [Test]
    public void Value_SetToInteger_ShouldBeExpected()
    {
        var variable = new Variable();

        variable.Value = 123;

        variable.Value.Should().Be(123);
        variable.Type.Should().Be(typeof(int));
        variable.Data.Should().Be("123");
    }

    [Test]
    public void Value_SetToDouble_ShouldBeExpected()
    {
        var variable = new Variable();

        variable.Value = 1.23;

        variable.Value.Should().Be(1.23);
        variable.Type.Should().Be(typeof(double));
        variable.Data.Should().Be("1.23");
    }

    [Test]
    public void Value_SetToBoolean_ShouldBeExpected()
    {
        var variable = new Variable();

        variable.Value = true;

        variable.Value.Should().Be(true);
        variable.Type.Should().Be(typeof(bool));
        variable.Data.Should().Be("True");
    }

    [Test]
    public void Value_SetToDint_ShouldBeExpected()
    {
        var variable = new Variable();

        variable.Value = new DINT(123);

        variable.Value.Should().Be(123);
        variable.Type.Should().Be(typeof(DINT));
        variable.Data.Should().Be("123");
    }

    [Test]
    public void Value_SetToReal_ShouldBeExpected()
    {
        var variable = new Variable();

        variable.Value = new REAL(1.23f);

        variable.Value.Should().Be(1.23f);
        variable.Type.Should().Be(typeof(REAL));
        variable.Data.Should().Be("1.23");
    }

    [Test]
    public void Value_SetToBool_ShouldBeExpected()
    {
        var variable = new Variable();

        variable.Value = new BOOL(true);

        variable.Value.Should().Be(true);
        variable.Type.Should().Be(typeof(BOOL));
        variable.Data.Should().Be("1");
    }

    [Test]
    public void Value_SetToLogixEnum_ShouldBeExpected()
    {
        var variable = new Variable();

        variable.Value = ExternalAccess.None;

        variable.Value.Should().Be(ExternalAccess.None);
        variable.Type.Should().Be(typeof(ExternalAccess));
        variable.Data.Should().Be("None");
    }

    [Test]
    public void Value_SetToRadix_ShouldBeExpected()
    {
        var variable = new Variable();

        variable.Value = Radix.Ascii;

        variable.Value.Should().Be(Radix.Ascii);
        variable.Type.Should().Be(Radix.Ascii.GetType());
        variable.Data.Should().Be("Ascii");
    }

    [Test]
    public void Value_SetToTag_ShouldBeExpected()
    {
        var variable = new Variable();

        var tag = new Tag { Name = "Test", Value = new TIMER() };

        variable.Value = tag;

        variable.Value.Should().BeEquivalentTo(tag);
        variable.Data.Should().Be(tag.Serialize().ToString());
        variable.Type.Should().Be(typeof(Tag));
    }
}