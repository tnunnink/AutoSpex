namespace AutoSpex.Engine.Tests;

[TestFixture]
public class VariableTests
{
    [Test]
    public void New_Default_ShouldBeExpected()
    {
        var variable = new Variable();
        
        variable.VariableId.Should().NotBeEmpty();
        variable.NodeId.Should().BeEmpty();
        variable.Name.Should().Be("Variable");
        variable.Type.Should().Be(typeof(string));
        variable.Group.Should().Be(TypeGroup.Text);
        variable.Value.Should().Be(string.Empty);
        variable.Data.Should().BeEmpty();
        variable.Description.Should().BeEmpty();
        variable.Formatted.Should().Be("{Variable}");
    }

    [Test]
    public void New_Overload_ShouldHaveExpectedValues()
    {
        var variable = new Variable("MyVar", "SomeValue", "This is a test variable.");
        
        variable.VariableId.Should().NotBeEmpty();
        variable.NodeId.Should().BeEmpty();
        variable.Name.Should().Be("MyVar");
        variable.Type.Should().Be(typeof(string));
        variable.Group.Should().Be(TypeGroup.Text);
        variable.Value.Should().Be("SomeValue");
        variable.Data.Should().Be("SomeValue");
        variable.Description.Should().Be("This is a test variable.");
        variable.Formatted.Should().Be("{MyVar}");
    }
    
    [Test]
    public void Value_SetToNull_ShouldBeExpected()
    {
        var variable = new Variable();

        variable.Value = null!;

        variable.Value.Should().Be(string.Empty);
        variable.Type.Should().Be(typeof(string));
        variable.Data.Should().Be(string.Empty);
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

        var tag = new Tag {Name = "Test", Value = new TIMER()};
        
        variable.Value = tag;
        
        variable.Value.Should().BeEquivalentTo(tag);
        variable.Type.Should().Be(tag.GetType());
        variable.Data.Should().Be(tag.Serialize().ToString());
    }
}