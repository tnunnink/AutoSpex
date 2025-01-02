namespace AutoSpex.Engine.Tests.Operations;

[TestFixture]
public class EmptyOperationTests
{
    [Test]
    public void Operator_WhenCalled_ShouldNotBeNull()
    {
        var operation = Operation.Empty;

        operation.Should().NotBeNull();
    }
    
    [Test]
    public void Name_WhenCalled_ShouldBeExpected()
    {
        var operation = Operation.Empty;

        operation.Name.Should().Be("Empty");
    }
    
    [Test]
    public void Execute_IsNull_ShouldBeFalse()
    {
        var operation = Operation.Empty;
        
        var result = operation.Execute(null);
        
        result.Should().BeFalse();
    }

    [Test]
    public void Execute_IsEmpty_ShouldBeTrue()
    {
        var operation = Operation.Empty;
        
        var result = operation.Execute("");
        
        result.Should().BeTrue();
    }
    
    [Test]
    public void Execute_IsNotEmpty_ShouldBeFalse()
    {
        var operation = Operation.Empty;
        
        var result = operation.Execute("Empty");
        
        result.Should().BeFalse();
    }
    
    [Test]
    public void Execute_IsNotString_ShouldBeFalse()
    {
        var operation = Operation.Empty;
        
        var result = operation.Execute(123);
        
        result.Should().BeFalse();
    }
}