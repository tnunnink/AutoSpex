namespace AutoSpex.Engine.Tests.Operations;

[TestFixture]
public class NullOperationTests
{
    [Test]
    public void Operator_WhenCalled_ShouldNotBeNull()
    {
        var operation = Operation.Null;

        operation.Should().NotBeNull();
    }
    
    [Test]
    public void Name_WhenCalled_ShouldBeExpected()
    {
        var operation = Operation.Null;

        operation.Name.Should().Be("Null");
    }

    [Test]
    public void Execute_IsNull_ShouldBeTrue()
    {
        var operation = Operation.Null;
        
        var result = operation.Execute(null);
        
        result.Should().BeTrue();
    }
    
    [Test]
    public void Execute_IsNotNull_ShouldBeFalse()
    {
        var operation = Operation.Null;
        
        var result = operation.Execute(123);
        
        result.Should().BeFalse();
    }
}