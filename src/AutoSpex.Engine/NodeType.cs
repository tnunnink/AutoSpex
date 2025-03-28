using Ardalis.SmartEnum;

// ReSharper disable ConvertIfStatementToReturnStatement

namespace AutoSpex.Engine;

public class NodeType : SmartEnum<NodeType, int>
{
    private NodeType(string name, int value) : base(name, value)
    {
    }

    public static readonly NodeType None = new(nameof(None), 0);
    public static readonly NodeType Collection = new(nameof(Collection), 1);
    public static readonly NodeType Container = new(nameof(Container), 2);
    public static readonly NodeType Spec = new(nameof(Spec), 3);

    /// <summary>
    /// Determines if the provided node type can be contained by the current node type.
    /// </summary>
    /// <param name="type">The node type to check.</param>
    /// <returns><c>true</c> if the node can be contained by this node type, otherwise, <c>false</c>.</returns>
    public bool CanContain(NodeType type)
    {
        if (type == Collection || type == None || this == Spec) return false;
        return true;
    }
}