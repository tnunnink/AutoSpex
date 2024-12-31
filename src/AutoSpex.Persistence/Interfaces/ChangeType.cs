using Ardalis.SmartEnum;

namespace AutoSpex.Engine;

public class ChangeType : SmartEnum<ChangeType, int>
{
    private ChangeType(string name, int value) : base(name, value)
    {
    }

    public static readonly ChangeType None = new(nameof(None), 0);
    public static readonly ChangeType Created = new(nameof(Created), 1);
    public static readonly ChangeType Deleted = new(nameof(Deleted), 2);
    public static readonly ChangeType Renamed = new(nameof(Renamed), 3);
    public static readonly ChangeType Updated = new(nameof(Updated), 4);
}