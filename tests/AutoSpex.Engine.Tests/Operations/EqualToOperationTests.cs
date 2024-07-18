namespace AutoSpex.Engine.Tests.Operations;

[TestFixture]
public class EqualToOperationTests
{
    [Test]
    public void Operator_EqualTo_ShouldNotBeNull()
    {
        var operation = Operation.EqualTo;

        operation.Should().NotBeNull();
    }
    
    [Test]
    public void Name_WhenCalled_ShouldBeExpected()
    {
        var operation = Operation.EqualTo;

        operation.Name.Should().Be("Equal To");
    }
    
    [Test]
    public void Equals_SameOperation_ShouldBeTrue()
    {
        var first = Operation.EqualTo;
        var second = Operation.EqualTo;
        
        var result = first.Equals(second);

        result.Should().BeTrue();
    }

    [Test]
    public void Evaluate_IsEqual_ShouldBeTrue()
    {
        var operation = Operation.EqualTo;
        
        var result = operation.Execute(1, 1);
        
        result.Should().BeTrue();
    }
    
    [Test]
    public void Evaluate_NotEqual_ShouldBeFalse()
    {
        var operation = Operation.EqualTo;
        
        var result = operation.Execute(1, 2);
        
        result.Should().BeFalse();
    }

    [Test]
    public void Execute_StringArgument_ShouldBeExpected()
    {
        var operation = Operation.EqualTo;

        var result = operation.Execute("Test", "Another");

        result.Should().BeFalse();
    }
}