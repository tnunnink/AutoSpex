namespace AutoSpex.Engine.Tests;

[TestFixture]
public class EvaluationTests
{
    [Test]
    public void Passed_ValidData_ShouldBeExpected()
    {
        var criterion = new Criterion("Name", Operation.Containing, "Test");
        var evaluation = Evaluation.Passed(criterion, new Tag("Test", 123), "Test");

        evaluation.Result.Should().Be(ResultState.Passed);
        evaluation.Message.Should().Be("Expected /Tag/Test to have Name Is Containing Test and found Test");
    }

    [Test]
    public void Failed_ValidData_ShouldBeExpected()
    {
        var criterion = new Criterion("Name", Operation.Containing, "Test");
        var evaluation = Evaluation.Failed(criterion, new Tag("Test", 123), "Fake");

        evaluation.Result.Should().Be(ResultState.Failed);
        evaluation.Message.Should().Be("Expected /Tag/Test to have Name Is Containing Test but found Fake");
    }

    [Test]
    public void Errored_ValidData_ShouldBeExpected()
    {
        var criterion = new Criterion("Name", Operation.Containing, "Test");
        var evaluation = Evaluation.Errored(criterion, new Tag("Test", 123),
            new InvalidOperationException("This evaluation failed to produce."));

        evaluation.Result.Should().Be(ResultState.Errored);
        evaluation.Message.Should()
            .Be("Expected /Tag/Test to have Name Is Containing Test but got error This evaluation failed to produce.");
    }
}