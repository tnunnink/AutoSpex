using System.Xml.Linq;
using L5Sharp.Core;

namespace AutoSpex.Engine;

public class IsEquivalentOperation() : BinaryOperation("IsEquivalent")
{
    public override string ShouldMessage => "Should Be Equivalent";

    protected override bool Evaluate(object? input, object value)
    {
        if (value is not string data) return false;

        if (input is not LogixElement element) return false;
        
        var other = XElement.Parse(data).Deserialize();
        return element.EquivalentTo(other);
    }
    
    protected override bool Supports(TypeGroup group) => group == TypeGroup.Element;
}