namespace AutoSpex.Engine.Tests.Operations;

[TestFixture]
public class ContainingOperationTests
{
    [Test]
    public void Operator_Any_ShouldNotBeNull()
    {
        var operation = Operation.Containing;

        operation.Should().NotBeNull();
    }
    
    [Test]
    public void Name_WhenCalled_ShouldBeExpected()
    {
        var operation = Operation.Containing;

        operation.Name.Should().Be(nameof(Operation.Containing));
    }
    
    [Test]
    public void Equals_SameOperation_ShouldBeTrue()
    {
        var first = Operation.Containing;
        var second = Operation.Containing;
        
        var result = first.Equals(second);

        result.Should().BeTrue();
    }
    
    [Test]
    public void Execute_StringArgumentDoesNotContain_ShouldBeFalse()
    {
        var operation = Operation.Containing;

        var result = operation.Execute("Test", "Another");

        result.Should().BeFalse();
    }
    
    [Test]
    public void Execute_StringArgumentDoesContain_ShouldBeExpected()
    {
        var operation = Operation.Containing;

        var result = operation.Execute("Something", "Some");

        result.Should().BeTrue();
    }

    [Test]
    public void Evaluate_CollectionArgDoesContain_ShouldBeTrue()
    {
        var items = new List<int> {1, 2, 3, 4, 5};
        
        var result = Operation.Containing.Execute(items, 3);
        
        result.Should().BeTrue();
    }
    
    [Test]
    public void Evaluate_CollectionArgDoesNotContain_ShouldBeFalse()
    {
        var items = new List<int> {1, 2, 3, 4, 5};
        
        var result = Operation.Containing.Execute(items, 6);
        
        result.Should().BeFalse();
    }
    
    [Test]
    public void Evaluate_CollectionStringArgDoesContain_ShouldBeTrue()
    {
        var items = new List<string> {"One", "Two", "Three"};
        
        var result = Operation.Containing.Execute(items, "Two");
        
        result.Should().BeTrue();
    }
    
    [Test]
    public void Evaluate_CollectionStringArgDoesNotContain_ShouldBeFalse()
    {
        var items = new List<string> {"One", "Two", "Three"};
        
        var result = Operation.Containing.Execute(items, "Four");
        
        result.Should().BeFalse();
    }
}