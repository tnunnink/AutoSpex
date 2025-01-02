namespace AutoSpex.Engine.Tests.Operations;

[TestFixture]
public class StartingWithOperationTests
{
    [Test]
    public void Operator_WhenCalled_ShouldNotBeNull()
    {
        var operation = Operation.StartingWith;

        operation.Should().NotBeNull();
    }

    [Test]
    public void Name_WhenCalled_ShouldBeExpected()
    {
        var operation = Operation.StartingWith;

        operation.Name.Should().Be("Starting With");
    }

    [Test]
    public void Execute_StartsWith_ShouldBeTrue()
    {
        var operation = Operation.StartingWith;

        var result = operation.Execute("This is a test", "This is");

        result.Should().BeTrue();
    }

    [Test]
    public void Execute_DoesNotStartsWith_ShouldBeFalse()
    {
        var operation = Operation.StartingWith;

        var result = operation.Execute("Here is a test", "This is");

        result.Should().BeFalse();
    }

    [Test]
    public void Execute_NullInput_ShouldBeFalse()
    {
        var operation = Operation.StartingWith;

        var result = operation.Execute(null, "Testing");

        result.Should().BeFalse();
    }

    [Test]
    public void Execute_NullArgument_ShouldThrowException()
    {
        var operation = Operation.StartingWith;

        FluentActions.Invoking(() => operation.Execute("This is a test"))
            .Should().Throw<ArgumentException>()
            .WithMessage($"Argument value required for {operation.Name} operation.");
    }
}