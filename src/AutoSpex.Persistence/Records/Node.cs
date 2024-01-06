using InvalidOperationException = System.InvalidOperationException;

namespace AutoSpex.Persistence;

public class Node
{
    private readonly List<Node> _nodes = [];

    private Node()
    {
        NodeId = Guid.NewGuid();
        ParentId = Guid.Empty;
        Parent = default;
        Name = string.Empty;
        Feature = Feature.Specifications;
        NodeType = NodeType.Collection;
        Depth = 0;
        Ordinal = 0;
    }

    public Guid NodeId { get; private init; }
    public Guid ParentId { get; private set; }
    public Node? Parent { get; private set; }
    public Feature Feature { get; private init; }
    public NodeType NodeType { get; private init; }
    public string Name { get; set; }
    public int Depth { get; private init; }
    public int Ordinal { get; set; }
    public IEnumerable<Node> Nodes => _nodes;


    public static Node Collection(Feature feature, string? name = default)
    {
        return new Node
        {
            NodeId = Guid.NewGuid(),
            ParentId = Guid.Empty,
            Parent = default,
            Feature = feature ?? throw new ArgumentNullException(nameof(feature)),
            NodeType = NodeType.Collection,
            Name = name ?? "New Collection"
        };
    }

    public void AddNode(Node node)
    {
        if (node is null) throw new ArgumentNullException(nameof(node));

        if (node.Feature != Feature)
            throw new InvalidOperationException(
                $"Can not add node to tree of different feature. This: {Feature}; Node: {node.Feature}");

        node.Parent = this;
        node.ParentId = NodeId;

        _nodes.Add(node);
    }

    public void RemoveNode(Node node)
    {
        if (node is null) throw new ArgumentNullException(nameof(node));

        node.Parent = default;
        node.ParentId = Guid.Empty;

        _nodes.Remove(node);
    }

    public Node NewFolder(string? name = default)
    {
        return new Node
        {
            NodeId = Guid.NewGuid(),
            ParentId = NodeId,
            Parent = this,
            Feature = Feature,
            NodeType = NodeType.Folder,
            Name = name ?? "New Folder",
            Depth = Depth + 1
        };
    }

    public Node NewSpec(string? name = default)
    {
        return new Node
        {
            NodeId = Guid.NewGuid(),
            ParentId = NodeId,
            Parent = this,
            Feature = Feature.Specifications,
            NodeType = NodeType.Spec,
            Name = name ?? "New Spec",
            Depth = Depth + 1
        };
    }

    public Node NewSource(string? name = default)
    {
        return new Node
        {
            NodeId = Guid.NewGuid(),
            ParentId = NodeId,
            Parent = this,
            Feature = Feature.Sources,
            NodeType = NodeType.Source,
            Name = name ?? "New Source",
            Depth = Depth + 1
        };
    }

    public override bool Equals(object? obj) => obj is Node other && other.NodeId == NodeId;
    public override int GetHashCode() => NodeId.GetHashCode();

    public static bool operator ==(Node? left, Node? right) => Equals(left, right);

    public static bool operator !=(Node? left, Node? right) => !Equals(left, right);
}