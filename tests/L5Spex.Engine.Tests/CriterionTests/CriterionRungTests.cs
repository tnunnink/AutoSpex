﻿using L5Sharp.Elements;
using L5Spex.Engine.Enumerations;
using L5Spex.Engine.Operations;

namespace L5Spex.Engine.Tests.CriterionTests;

[TestFixture]
public class CriterionRungTests
{
    [Test]
    public void Evaluate_TextEqualTo_IsEqualTo_ShouldBeTrue()
    {
        var criterion = Criterion.For<Rung>("Text", Operation.EqualTo, "XIC(Input)OTE(Output);");
        var rung = new Rung {Text = "XIC(Input)OTE(Output);"};
        
        var evaluation = criterion.Evaluate(rung);
        
        evaluation.Result.Should().Be(ResultType.Passed);
    }
}