namespace AutoSpex.Engine.Tests.Operations;

public class AnyOperationTests
{
    [Test]
    public void Operator_Any_ShouldNotBeNull()
    {
        var operation = Operation.Any;

        operation.Should().NotBeNull();
    }
    
    [Test]
    public void Name_WhenCalled_ShouldBeExpected()
    {
        var operation = Operation.Any;

        operation.Name.Should().Be(nameof(Operation.Any));
    }

    [Test]
    public void Execute_HasSpecifiedValue_ShouldBeTrue()
    {
        var items = new List<int> {1, 2, 3, 4, 5};
        
        var result = Operation.Any.Execute(items, new Criterion(Operation.EqualTo, 3));
        
        result.Should().BeTrue();
    }
    
    [Test]
    public void Execute_DoesNotHaveSpecifiedValue_ShouldBeFalse()
    {
        var items = new List<int> {1, 2, 3, 4, 5};
        
        var result = Operation.Any.Execute(items, new Criterion(Operation.EqualTo, 6));
        
        result.Should().BeFalse();
    }

    [Test]
    public void Execute_WithInnerOperation_ShouldReturnFalse()
    {
        var items = new List<int> {1, 2, 3, 4, 5};

        var result = Operation.Any.Execute(items, new Criterion(Operation.GreaterThan, 10));

        result.Should().BeFalse();
    }
    
    [Test]
    public void Execute_WithInnerOperation_ShouldReturnTrue()
    {
        var items = new List<int> {1, 2, 3, 4, 5};

        var result = Operation.Any.Execute(items, new Criterion(Operation.LessThan, 10));

        result.Should().BeTrue();
    }
}