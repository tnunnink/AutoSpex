namespace AutoSpex.Engine.Tests;

[TestFixture]
public class VariableTests
{
    [Test]
    public void New_Default_ShouldBeExpected()
    {
        var variable = new Variable("MyVar", "Test");
        
        variable.VariableId.Should().NotBeEmpty();
        variable.Name.Should().Be("MyVar");
        variable.Value.Should().Be("Test");
        variable.Description.Should().BeNull();
    }

    [Test]
    public void New_WithDescription_ShouldHaveExpectedValues()
    {
        var variable = new Variable("MyVar", "Test", "This is a test variable for my spec");
        
        variable.VariableId.Should().NotBeEmpty();
        variable.Name.Should().Be("MyVar");
        variable.Value.Should().Be("Test");
        variable.Description.Should().Be("This is a test variable for my spec");
    }

    [Test]
    public void Value_Set()
    {
        
    }
}