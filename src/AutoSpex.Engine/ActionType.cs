using Ardalis.SmartEnum;

namespace AutoSpex.Engine;

public class ActionType : SmartEnum<ActionType, int>
{
    private ActionType(string name, int value) : base(name, value)
    {
    }

    public static readonly ActionType Suppress = new(nameof(Suppress), 1);
    public static readonly ActionType Override = new(nameof(Override), 2);
}