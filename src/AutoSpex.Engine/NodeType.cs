using Ardalis.SmartEnum;

namespace AutoSpex.Engine;

public class NodeType : SmartEnum<NodeType, int>
{
    private NodeType(string name, int value) : base(name, value)
    {
    }

    public static readonly NodeType Container = new(nameof(Container), 1);
    public static readonly NodeType Spec = new(nameof(Spec), 2);
    public static readonly NodeType Source = new(nameof(Source), 3);
    public static readonly NodeType Run = new(nameof(Run), 4);
}