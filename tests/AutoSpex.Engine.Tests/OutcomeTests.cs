using System.Text.Json;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Engine.Tests;

[TestFixture]
public class OutcomeTests
{
    [Test]
    public void New_DefaultOverload_ShouldBeExpected()
    {
        var outcome = new Outcome();

        outcome.OutcomeId.Should().NotBeEmpty();
        outcome.NodeId.Should().BeEmpty();
        outcome.Name.Should().BeEmpty();
        outcome.Path.Should().BeEmpty();
        outcome.Result.Should().Be(ResultState.None);
        outcome.Duration.Should().Be(0);
        outcome.Evaluations.Should().BeEmpty();
    }

    [Test]
    public Task Serialized_Verification_ShouldBeVerified()
    {
        var criterion = new Criterion("Name", Operation.Containing, "Test");
        var evaluation = Evaluation.Passed(criterion, new Tag("Test", 123), "Test");
        var verification = Verification.For(evaluation);
        var outcome = new Outcome
        {
            NodeId = Guid.NewGuid(),
            RunId = Guid.NewGuid(),
            Name = "Testing",
            Path = "This/Is/A/Test"
        };
        outcome.Apply(verification);

        var json = JsonSerializer.Serialize(outcome);

        return VerifyJson(json);
    }

    [Test]
    public void Deserialize_JosnString_ShouldNotBeNull()
    {
        var criterion = new Criterion("Name", Operation.Containing, "Test");
        var evaluation = Evaluation.Passed(criterion, new Tag("Test", 123), "Test");
        var verification = Verification.For(evaluation);
        var outcome = new Outcome
        {
            NodeId = Guid.NewGuid(),
            RunId = Guid.NewGuid(),
            Name = "Testing",
            Path = "This/Is/A/Test"
        };
        outcome.Apply(verification);

        var json = JsonSerializer.Serialize(outcome);

        var result = JsonSerializer.Deserialize<Outcome>(json);

        result.Should().NotBeNull();
    }

    [Test]
    public void Deserialize_JosnString_ShouldBeEquivalentToOriginal()
    {
        var criterion = new Criterion("Name", Operation.Containing, "Test");
        var evaluation = Evaluation.Passed(criterion, new Tag("Test", 123), "Test");
        var verification = Verification.For(evaluation);
        var outcome = new Outcome
        {
            NodeId = Guid.NewGuid(),
            RunId = Guid.NewGuid(),
            Name = "Testing",
            Path = "This/Is/A/Test"
        };
        outcome.Apply(verification);
        var json = JsonSerializer.Serialize(outcome);

        var result = JsonSerializer.Deserialize<Outcome>(json);

        result?.Result.Should().Be(ResultState.Passed);
        result?.Duration.Should().Be(0);
        result?.PassRate.Should().Be(0);
        result?.Evaluations.Should().BeEmpty();
    }
}