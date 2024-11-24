namespace AutoSpex.Engine.Tests.Operations;

[TestFixture]
public class EqualToOperationTests
{
    [Test]
    public void Operator_WhenCalled_ShouldNotBeNull()
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
    public void Execute_IsEqual_ShouldBeTrue()
    {
        var operation = Operation.EqualTo;
        
        var result = operation.Execute(1, 1);
        
        result.Should().BeTrue();
    }
    
    [Test]
    public void Execute_NotEqual_ShouldBeFalse()
    {
        var operation = Operation.EqualTo;
        
        var result = operation.Execute(1, 2);
        
        result.Should().BeFalse();
    }
    
    [Test]
    public void Execute_NullArgument_ShouldThrowArgumentExcetpion()
    {
        var operation = Operation.EqualTo;

        FluentActions.Invoking(() => operation.Execute(1))
            .Should().Throw<ArgumentException>()
            .WithMessage($"Argument value required for {operation.Name} operation.");
    }

    [Test]
    public void Execute_StringArgument_ShouldBeExpected()
    {
        var operation = Operation.EqualTo;

        var result = operation.Execute("Test", "Another");

        result.Should().BeFalse();
    }
    
    [Test]
    public void Execute_TagMemebers_ShouldBeTrue()
    {
        var content = L5X.Load(Known.Test);
        var first = content.Get<Tag>("TestComplexTag").Clone();
        var second = content.Get<Tag>("TestComplexTag").Clone();
        second.Description = "Testing...";

        var result = Operation.EqualTo.Execute(first["SimpleMember"].Value, second["SimpleMember"].Value);

        result.Should().BeTrue();
    }
}