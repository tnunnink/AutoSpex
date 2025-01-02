namespace AutoSpex.Engine.Tests.Operations;

[TestFixture]
public class BetweenOperationTests
{
    [Test]
    public void Operator_WhenCalled_ShouldNotBeNull()
    {
        var operation = Operation.Between;

        operation.Should().NotBeNull();
    }

    [Test]
    public void Name_WhenCalled_ShouldBeExpected()
    {
        var operation = Operation.Between;

        operation.Name.Should().Be("Between");
    }

    [Test]
    public void Execute_IsGreater_ShouldBeFalse()
    {
        var operation = Operation.Between;

        var result = operation.Execute(12, new Range(1, 10));

        result.Should().BeFalse();
    }

    [Test]
    public void Execute_IsLess_ShouldBeFalse()
    {
        var operation = Operation.Between;

        var result = operation.Execute(0, new Range(1, 10));

        result.Should().BeFalse();
    }

    [Test]
    public void Execute_IsBetween_ShouldBeTrue()
    {
        var operation = Operation.Between;

        var result = operation.Execute(5, new Range(1, 10));

        result.Should().BeTrue();
    }

    [Test]
    public void Execute_IsMinValue_ShouldBeTrue()
    {
        var operation = Operation.Between;

        var result = operation.Execute(1, new Range(1, 10));

        result.Should().BeTrue();
    }

    [Test]
    public void Execute_IsMaxValue_ShouldBeTrue()
    {
        var operation = Operation.Between;

        var result = operation.Execute(10, new Range(1, 10));

        result.Should().BeTrue();
    }

    [Test]
    public void Execute_NullInput_ShouldBeFalse()
    {
        var operation = Operation.Between;

        var result = operation.Execute(null, new Range(1, 10));

        result.Should().BeFalse();
    }

    [Test]
    public void Execute_NullArgument_ShouldThrowArgumentExcetpion()
    {
        var operation = Operation.Between;

        FluentActions.Invoking(() => operation.Execute(1))
            .Should().Throw<ArgumentException>()
            .WithMessage($"Argument value required for Between operation.");
    }

    [Test]
    public void Execute_IsInputNotIComparable_ShouldThrowException()
    {
        var operation = Operation.Between;

        FluentActions.Invoking(() => operation.Execute(new Tag(), new Range(1, 10)))
            .Should().Throw<InvalidOperationException>()
            .WithMessage($"Input type {typeof(Tag)} is not a comparable type.");
    }

    [Test]
    public void Execute_ArgumentNotRange_ShouldThrowException()
    {
        var operation = Operation.Between;

        FluentActions.Invoking(() => operation.Execute(1, new List<int> { 1, 2 }))
            .Should().Throw<ArgumentException>()
            .WithMessage($"Between operation expects a {typeof(Range)} argument.");
    }
}