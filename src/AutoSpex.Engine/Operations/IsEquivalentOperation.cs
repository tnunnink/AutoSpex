using System.Xml.Linq;
using L5Sharp.Core;

namespace AutoSpex.Engine;

public class IsEquivalentOperation() : BinaryOperation("IsEquivalent")
{
    public override string ShouldMessage => "Should Be Equivalent";

    protected override bool Evaluate(object? input, object value)
    {
        if (value is not string data) return false;
        
        switch (input)
        {
            case LogixElement element:
            {
                var other = XElement.Parse(data).Deserialize();
                return element.IsEquivalent(other);
            }
            case LogixType type:
            {
                var xml = XElement.Parse(data);
                var other = LogixData.Deserialize(xml);
                return type.IsEquivalent(other);
            }
            default:
                return false;
        }
    }
    
    protected override bool Supports(TypeGroup group) => group == TypeGroup.Element;
}