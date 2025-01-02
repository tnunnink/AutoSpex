namespace AutoSpex.Engine.Tests.Operations;

[TestFixture]
public class EndingWithOperationTests
{
    [Test]
    public void Operator_WhenCalled_ShouldNotBeNull()
    {
        var operation = Operation.EndingWith;

        operation.Should().NotBeNull();
    }

    [Test]
    public void Name_WhenCalled_ShouldBeExpected()
    {
        var operation = Operation.EndingWith;

        operation.Name.Should().Be("Ending With");
    }

    [Test]
    public void Execute_EndsWith_ShouldBeTrue()
    {
        var operation = Operation.EndingWith;

        var result = operation.Execute("This is a test", "a test");

        result.Should().BeTrue();
    }

    [Test]
    public void Execute_DoesNotEndWith_ShouldBeFalse()
    {
        var operation = Operation.EndingWith;

        var result = operation.Execute("Here is a test", "is a");

        result.Should().BeFalse();
    }

    [Test]
    public void Execute_NullInput_ShouldBeFalse()
    {
        var operation = Operation.EndingWith;

        var result = operation.Execute(null, "Testing");

        result.Should().BeFalse();
    }

    [Test]
    public void Execute_NullArgument_ShouldThrowException()
    {
        var operation = Operation.EndingWith;

        FluentActions.Invoking(() => operation.Execute("This is a test"))
            .Should().Throw<ArgumentException>()
            .WithMessage($"Argument value required for {operation.Name} operation.");
    }
}