using L5Spex.Engine.Enumerations;
using L5Spex.Engine.Operations;

namespace L5Spex.Engine;

public class PredicateInput
{
    public Type Type { get; set; }
    public string Property { get; set; }
    public Operation Operation { get; set; }
    public object[] Arguments { get; set; }
    public ChainType Chain { get; set; }
}