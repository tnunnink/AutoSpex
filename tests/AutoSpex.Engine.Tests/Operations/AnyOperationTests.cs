﻿namespace AutoSpex.Engine.Tests.Operations;

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
    public void Equals_SameOperation_ShouldBeTrue()
    {
        var first = Operation.Any;
        var second = Operation.Any;
        
        var result = first.Equals(second);

        result.Should().BeTrue();
    }

    [Test]
    public void Evaluate_HasSpecifiedValue_ShouldBeTrue()
    {
        var items = new List<int> {1, 2, 3, 4, 5};
        
        var result = Operation.Any.Execute(items, new Criterion(Property.Default, Operation.EqualTo, 3));
        
        result.Should().BeTrue();
    }
    
    [Test]
    public void Evaluate_DoesNotHaveSpecifiedValue_ShouldBeFalse()
    {
        var items = new List<int> {1, 2, 3, 4, 5};
        
        var result = Operation.Any.Execute(items, new Criterion(Property.Default, Operation.EqualTo, 6));
        
        result.Should().BeFalse();
    }

    [Test]
    public void Evaluate_WithInnerOperation_ShouldReturnFalse()
    {
        var items = new List<int> {1, 2, 3, 4, 5};

        var result = Operation.Any.Execute(items, new Criterion(Property.Default, Operation.GreaterThan, 10));

        result.Should().BeFalse();
    }
    
    [Test]
    public void Evaluate_WithInnerOperation_ShouldReturnTrue()
    {
        var items = new List<int> {1, 2, 3, 4, 5};

        var result = Operation.Any.Execute(items, new Criterion(Property.Default, Operation.LessThan, 10));

        result.Should().BeTrue();
    }
}