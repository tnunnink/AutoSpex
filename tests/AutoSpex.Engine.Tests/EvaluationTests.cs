using System.Text.Json;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Engine.Tests;

[TestFixture]
public class EvaluationTests
{
    [Test]
    public void Passed_ValidData_ShouldBeExpected()
    {
        var criterion = new Criterion(Element.Tag.Property("Name"), Operation.Containing, "Test");
        var evaluation = Evaluation.Passed(criterion, new Tag("Test", 123), "Test");

        evaluation.Result.Should().Be(ResultState.Passed);
        evaluation.Candidate.Should().Be("/Tag/Test");
        evaluation.Criteria.Should().Be("Name Is Containing");
        evaluation.Expected.Should().HaveCount(1);
        evaluation.Actual.Should().Be("Test");
        evaluation.Error.Should().BeNull();
    }

    [Test]
    public void Failed_ValidData_ShouldBeExpected()
    {
        var criterion = new Criterion(Element.Tag.Property("Name"), Operation.Containing, "Test");
        var evaluation = Evaluation.Failed(criterion, new Tag("Test", 123), "Fake");

        evaluation.Result.Should().Be(ResultState.Failed);
        evaluation.Candidate.Should().Be("/Tag/Test");
        evaluation.Criteria.Should().Be("Name Is Containing");
        evaluation.Expected.Should().HaveCount(1);
        evaluation.Actual.Should().Be("Fake");
        evaluation.Error.Should().BeNull();
    }

    [Test]
    public void Errored_ValidData_ShouldBeExpected()
    {
        var criterion = new Criterion(Element.Tag.Property("Name"), Operation.Containing, "Test");
        var evaluation = Evaluation.Errored(criterion, new Tag("Test", 123),
            new InvalidOperationException("This evaluaiton failed to produce."));

        evaluation.Result.Should().Be(ResultState.Errored);
        evaluation.Candidate.Should().Be("/Tag/Test");
        evaluation.Criteria.Should().Be("Name Is Containing");
        evaluation.Expected.Should().HaveCount(1);
        evaluation.Actual.Should().BeEmpty();
        evaluation.Error.Should().Be("This evaluaiton failed to produce.");
    }

    [Test]
    public Task Serialized_Evaluation_ShouldBeVerified()
    {
        var criterion = new Criterion(Element.Tag.Property("Name"), Operation.Containing, "Test");
        var evaluation = Evaluation.Passed(criterion, new Tag("Test", 123), "Test");

        var json = JsonSerializer.Serialize(evaluation);

        return VerifyJson(json);
    }

    [Test]
    public void Deserialize_JosnString_ShouldNotBeNull()
    {
        var criterion = new Criterion(Element.Tag.Property("Name"), Operation.Containing, "Test");
        var evaluation = Evaluation.Passed(criterion, new Tag("Test", 123), "Test");
        var json = JsonSerializer.Serialize(evaluation);

        var result = JsonSerializer.Deserialize<Evaluation>(json);

        result.Should().NotBeNull();
    }

    [Test]
    public void Deserialize_JosnString_ShouldBeEquivalentToOriginal()
    {
        var criterion = new Criterion(Element.Tag.Property("Name"), Operation.Containing, "Test");
        var evaluation = Evaluation.Passed(criterion, new Tag("Test", 123), "Test");
        var json = JsonSerializer.Serialize(evaluation);

        var result = JsonSerializer.Deserialize<Evaluation>(json);

        result.Should().BeEquivalentTo(evaluation);
    }
}