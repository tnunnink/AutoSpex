namespace AutoSpex.Engine;

public record NodeInfo(Guid NodeId, NodeType Type, string Name)
{
    public NodeInfo(Node node) : this(node.NodeId, node.Type, node.Name)
    {
    }

    public static NodeInfo Empty => new(Guid.Empty, NodeType.None, string.Empty);
    public static implicit operator NodeInfo(Node node) => new(node);
}