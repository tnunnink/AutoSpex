using Ardalis.SmartEnum;

namespace AutoSpex.Engine;

public class ChainType : SmartEnum<ChainType, int>
{
    private ChainType(string name, int value) : base(name, value)
    {
    }

    public static ChainType And => new(nameof(And), 1);
    
    public static ChainType Or => new(nameof(Or), 2);
}