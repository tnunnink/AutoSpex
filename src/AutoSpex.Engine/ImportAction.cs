using Ardalis.SmartEnum;

namespace AutoSpex.Engine;

public class ImportAction : SmartEnum<ImportAction, int>
{
    private ImportAction(string name, int value) : base(name, value)
    {
    }

    public static readonly ImportAction None = new(nameof(None), 0);
    public static readonly ImportAction Cancel = new(nameof(Cancel), 1);
    public static readonly ImportAction Replace = new(nameof(Replace), 2);
    public static readonly ImportAction Copy = new(nameof(Copy), 3);
}