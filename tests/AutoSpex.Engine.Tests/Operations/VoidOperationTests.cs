namespace AutoSpex.Engine.Tests.Operations;

[TestFixture]
public class VoidOperationTests
{
    [Test]
    public void Operator_WhenCalled_ShouldNotBeNull()
    {
        var operation = Operation.Void;

        operation.Should().NotBeNull();
    }
    
    [Test]
    public void Name_WhenCalled_ShouldBeExpected()
    {
        var operation = Operation.Void;

        operation.Name.Should().Be("Void");
    }
    
    [Test]
    public void Execute_IsNull_ShouldBeTrue()
    {
        var operation = Operation.Void;
        
        var result = operation.Execute(null);
        
        result.Should().BeTrue();
    }

    [Test]
    public void Execute_IsEmpty_ShouldBeTrue()
    {
        var operation = Operation.Void;
        
        var result = operation.Execute("");
        
        result.Should().BeTrue();
    }
    
    [Test]
    public void Execute_IsWhiteSpace_ShouldBeTrue()
    {
        var operation = Operation.Void;
        
        var result = operation.Execute("   ");
        
        result.Should().BeTrue();
    }
    
    [Test]
    public void Execute_IsNotVoid_ShouldBeFalse()
    {
        var operation = Operation.Void;
        
        var result = operation.Execute("Void");
        
        result.Should().BeFalse();
    }
    
    [Test]
    public void Execute_IsNotString_ShouldBeFalse()
    {
        var operation = Operation.Void;
        
        var result = operation.Execute(123);
        
        result.Should().BeFalse();
    }
}