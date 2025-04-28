namespace AutoSpex.Engine.Tests;

[TestFixture]
public class EvaluationTests
{
    [Test]
    public void Passed_ValidData_ShouldBeExpected()
    {
        var evaluation = Evaluation.Passed("Name Is Containing", "Test", "Test");

        evaluation.Result.Should().Be(ResultState.Passed);
        evaluation.Criteria.Should().Be("Name Is Containing");
        evaluation.Expected.Should().Be("Test");
        evaluation.Returned.Should().Be("Test");
        evaluation.ToString().Should().Be("Expected Name Is Containing Test and found Test");
    }

    [Test]
    public void Failed_ValidData_ShouldBeExpected()
    {
        var evaluation = Evaluation.Failed("Name Is Containing", "Test", "Fake");

        evaluation.Result.Should().Be(ResultState.Failed);
        evaluation.Criteria.Should().Be("Name Is Containing");
        evaluation.Expected.Should().Be("Test");
        evaluation.Returned.Should().Be("Fake");
        evaluation.ToString().Should().Be("Expected Name Is Containing Test but found Fake");
    }

    [Test]
    public void Errored_ValidData_ShouldBeExpected()
    {
        var exception = new InvalidOperationException("This evaluation failed to produce.");
        var evaluation = Evaluation.Errored("Name Is Containing", "Test", exception);

        evaluation.Result.Should().Be(ResultState.Errored);
        evaluation.Criteria.Should().Be("Name Is Containing");
        evaluation.Expected.Should().Be("Test");
        evaluation.Returned.Should().Be("This evaluation failed to produce.");
        evaluation.ToString().Should()
            .Be("Expected Name Is Containing Test but got error This evaluation failed to produce.");
    }
}