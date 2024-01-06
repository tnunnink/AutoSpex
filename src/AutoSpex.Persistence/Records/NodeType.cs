using Ardalis.SmartEnum;

namespace AutoSpex.Persistence;

public class NodeType : SmartEnum<NodeType, int>
{
    private NodeType(string name, int value) : base(name, value)
    {
    }

    public static readonly NodeType Collection = new(nameof(Collection), 1);
    public static readonly NodeType Folder = new(nameof(Folder), 2);
    public static readonly NodeType Spec = new(nameof(Spec), 3);
    public static readonly NodeType Source = new(nameof(Source), 4);
    public static readonly NodeType Runner = new(nameof(Runner), 5);
}