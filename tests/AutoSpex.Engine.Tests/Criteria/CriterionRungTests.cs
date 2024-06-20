namespace AutoSpex.Engine.Tests.Criteria;

[TestFixture]
public class CriterionRungTests
{
    [Test]
    public void Evaluate_TextEqualTo_IsEqualTo_ShouldBeTrue()
    {
        var criterion = new Criterion(Element.Rung.Property("Text"), Operation.Equal, "XIC(Input)OTE(Output);");
        var rung = new Rung {Text = "XIC(Input)OTE(Output);"};
        
        var evaluation = criterion.Evaluate(rung);
        
        evaluation.Result.Should().Be(ResultState.Passed);
    }
}