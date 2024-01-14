using Ardalis.SmartEnum;

namespace AutoSpex.Engine;

public class NodeType : SmartEnum<NodeType, int>
{
    private NodeType(string name, int value) : base(name, value)
    {
    }

    public static readonly NodeType Collection = new(nameof(Collection), 1);
    public static readonly NodeType Folder = new(nameof(Folder), 2);
    public static readonly NodeType Spec = new(nameof(Spec), 3);

    /// <summary>
    /// Determines if the provided <see cref="NodeType"/> can be added as a child to this <c>NodeType</c> 
    /// </summary>
    /// <param name="nodeType">The <see cref="NodeType"/> to check.</param>
    /// <returns>Returns true if this node's value is a lower than the provided node, meaning it accepts the node
    /// type as one that can be added as a child; Otherwise, <c>false</c>.</returns>
    public bool CanAdd(NodeType nodeType) => (this == Collection || this == Folder) && nodeType != Collection;
}