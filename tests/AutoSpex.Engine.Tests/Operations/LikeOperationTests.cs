namespace AutoSpex.Engine.Tests.Operations;

[TestFixture]
public class LikeOperationTests
{
    [Test]
    public void Operator_WhenCalled_ShouldNotBeNull()
    {
        var operation = Operation.Like;

        operation.Should().NotBeNull();
    }
    
    [Test]
    public void Name_WhenCalled_ShouldBeExpected()
    {
        var operation = Operation.Like;

        operation.Name.Should().Be("Like");
    }

    [Test]
    public void Execute_IsLikePattern_ShouldBeTrue()
    {
        var operation = Operation.Like;
        
        var result = operation.Execute("This is a test string", "*test*");
        
        result.Should().BeTrue();
    }
    
    [Test]
    public void Execute_IsNotLikePattern_ShouldBeFalse()
    {
        var operation = Operation.Like;
        
        var result = operation.Execute("This is a test string", "*tst*");
        
        result.Should().BeFalse();
    }
    
    [Test]
    public void Execute_NullInput_ShouldBeFalse()
    {
        var operation = Operation.Like;

        var result = operation.Execute(null, "*tst*");

        result.Should().BeFalse();
    }

    [Test]
    public void Execute_NullArgument_ShouldThrowException()
    {
        var operation = Operation.Like;

        FluentActions.Invoking(() => operation.Execute("This is a test"))
            .Should().Throw<ArgumentException>()
            .WithMessage($"Argument value required for {operation.Name} operation.");
    }
}