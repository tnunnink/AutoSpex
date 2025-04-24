namespace AutoSpex.Engine.Tests;

[TestFixture]
public class VerificationTests
{
    [Test]
    public void New_NoEvaluations_ShouldBePassedWithNonNullObject()
    {
        var verification = new Verification("Test", []);

        verification.Result.Should().Be(ResultState.Passed);
        verification.Candidate.Should().NotBeNull();
        verification.Evaluations.Should().BeEmpty();
    }

    [Test]
    public void New_WithEvaluations_ShouldBeExpected()
    {
        var candidate = new Tag("Test", 123);
        var evaluation = Evaluation.Passed(new Criterion("Value", Operation.EqualTo, 123), candidate, 123);
        
        var verification = new Verification(new Tag("Test", 123), [evaluation]);

        verification.Result.Should().Be(ResultState.Passed);
        verification.Candidate.Should().NotBeNull();
        verification.Evaluations.Should().HaveCount(1);
    }
}