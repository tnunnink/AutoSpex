namespace AutoSpex.Engine.Tests.Operations;

[TestFixture]
public class LessThanOperationTests
{
    [Test]
    public void Operator_WhenCalled_ShouldNotBeNull()
    {
        var operation = Operation.LessThan;

        operation.Should().NotBeNull();
    }

    [Test]
    public void Name_WhenCalled_ShouldBeExpected()
    {
        var operation = Operation.LessThan;

        operation.Name.Should().Be("Less Than");
    }

    [Test]
    public void Execute_IsGreater_ShouldBeFalse()
    {
        var operation = Operation.LessThan;

        var result = operation.Execute(2, 1);

        result.Should().BeFalse();
    }

    [Test]
    public void Execute_IsLess_ShouldBeTrue()
    {
        var operation = Operation.LessThan;

        var result = operation.Execute(2, 12);

        result.Should().BeTrue();
    }

    [Test]
    public void Execute_IsEqual_ShouldBeFalse()
    {
        var operation = Operation.LessThan;

        var result = operation.Execute(2, 2);

        result.Should().BeFalse();
    }

    [Test]
    public void Execute_NullInput_ShouldBeFalse()
    {
        var operation = Operation.LessThan;

        var result = operation.Execute(null, 2);

        result.Should().BeFalse();
    }

    [Test]
    public void Execute_NullArgument_ShouldThrowArgumentExcetpion()
    {
        var operation = Operation.LessThan;

        FluentActions.Invoking(() => operation.Execute(1))
            .Should().Throw<ArgumentException>()
            .WithMessage($"Argument value required for {operation.Name} operation.");
    }

    [Test]
    public void Execute_IsInputNotIComparable_ShouldThrowException()
    {
        var operation = Operation.LessThan;

        FluentActions.Invoking(() => operation.Execute(new Tag(), 1))
            .Should().Throw<InvalidOperationException>()
            .WithMessage($"Input type {typeof(Tag)} is not a comparable type.");
    }

    [Test]
    public void Execute_IsArgumentNotIComparable_ShouldThrowException()
    {
        var operation = Operation.LessThan;

        FluentActions.Invoking(() => operation.Execute(1, new Tag()))
            .Should().Throw<ArgumentException>()
            .WithMessage("Object must be of type Int32.");
    }
}