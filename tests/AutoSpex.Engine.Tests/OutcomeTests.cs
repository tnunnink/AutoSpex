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
        outcome.Verification.Result.Should().Be(ResultState.None);
        outcome.Verification.Duration.Should().Be(0);
        outcome.Verification.Evaluations.Should().BeEmpty();
    }
}