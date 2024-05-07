using JetBrains.Annotations;

namespace AutoSpex.Engine;

[PublicAPI]
public class Node : IEquatable<Node>
{
    private readonly List<Node> _nodes = [];

    private Node()
    {
        NodeId = Guid.NewGuid();
        ParentId = Guid.Empty;
        Parent = default;
        NodeType = NodeType.Collection;
        Name = string.Empty;
        Depth = 0;
    }

    public Guid NodeId { get; private init; }
    public Guid ParentId { get; private set; }
    public Node? Parent { get; private set; }
    public NodeType NodeType { get; private init; }
    public string Name { get; set; }
    public string Documentation { get; set; } = string.Empty;
    public int Depth { get; private set; }
    public string Path => GetPath();
    public Node? Collection => GetCollection();
    public IEnumerable<Node> Nodes => _nodes;

    public static Node NewCollection(string? name = default)
    {
        return new Node
        {
            NodeId = Guid.NewGuid(),
            ParentId = Guid.Empty,
            Parent = default,
            NodeType = NodeType.Collection,
            Name = name ?? "New Collection"
        };
    }

    public static Node NewFolder(string? name = default)
    {
        return new Node
        {
            NodeId = Guid.NewGuid(),
            ParentId = Guid.Empty,
            Parent = default,
            NodeType = NodeType.Folder,
            Name = name ?? "New Folder"
        };
    }

    public static Node NewSpec(string? name = default, Action<Spec>? config = default)
    {
        return new Node
        {
            NodeId = Guid.NewGuid(),
            ParentId = Guid.Empty,
            Parent = default,
            NodeType = NodeType.Spec,
            Name = name ?? "New Spec",
        };
    }

    public Node AddFolder(string? name = default)
    {
        if (!NodeType.CanAdd(NodeType.Folder))
            throw new ArgumentException($"Can not add a {NodeType.Folder} to a {NodeType} node.");

        var node = new Node
        {
            NodeId = Guid.NewGuid(),
            ParentId = NodeId,
            Parent = this,
            NodeType = NodeType.Folder,
            Name = name ?? "New Folder",
            Depth = Depth + 1
        };

        _nodes.Add(node);
        return node;
    }

    public Node AddSpec(string? name = default, Action<Spec>? config = default)
    {
        if (!NodeType.CanAdd(NodeType.Spec))
            throw new ArgumentException($"Can not add a {NodeType.Spec} to a {NodeType} node.");

        var node = new Node
        {
            NodeId = Guid.NewGuid(),
            ParentId = NodeId,
            Parent = this,
            NodeType = NodeType.Spec,
            Name = name ?? "New Spec",
            Depth = Depth + 1
        };

        _nodes.Add(node);
        return node;
    }

    public void AddNode(Node node)
    {
        if (node is null)
            throw new ArgumentNullException(nameof(node));

        if (!NodeType.CanAdd(node.NodeType))
            throw new ArgumentException($"Can not add a {node.NodeType} to a {NodeType} node.");

        node.Parent?.RemoveNode(node);

        node.Parent = this;
        node.ParentId = NodeId;
        node.Depth = Depth + 1;

        _nodes.Add(node);
    }

    public void RemoveNode(Node node)
    {
        if (node is null)
            throw new ArgumentNullException(nameof(node));

        node.Parent = default;
        node.ParentId = Guid.Empty;

        _nodes.Remove(node);
    }

    public void ClearNodes()
    {
        foreach (var node in _nodes)
        {
            node.Parent = default;
            node.ParentId = Guid.Empty;
        }

        _nodes.Clear();
    }

    public IEnumerable<Node> Ancestors()
    {
        var nodes = new List<Node>();

        var current = Parent;
        while (current is not null)
        {
            nodes.Add(current);
            current = current.Parent;
        }

        return nodes;
    }

    public IEnumerable<Node> AncestralPath()
    {
        var nodes = new List<Node>();

        var current = this;
        while (current is not null)
        {
            nodes.Add(current);
            current = current.Parent;
        }

        return nodes.OrderBy(n => n.Depth);
    }

    public IEnumerable<Node> Descendents()
    {
        List<Node> nodes = [];

        foreach (var node in _nodes)
        {
            nodes.Add(node);
            nodes.AddRange(node.Descendents());
        }

        return nodes;
    }

    /// <summary>
    /// Gets all descendent spec nodes, including this node if it is a spec type node.
    /// </summary>
    /// <returns>A collection of <see cref="Node"/> of type <see cref="NodeType.Spec"/>.</returns>
    public IEnumerable<Node> Specs()
    {
        List<Node> nodes = [];

        if (NodeType == NodeType.Spec)
            nodes.Add(this);

        nodes.AddRange(_nodes.SelectMany(n => n.Specs()));
        return nodes;
    }

    private Node? GetCollection()
    {
        var current = this;

        while (current is not null && current.NodeType != NodeType.Collection)
        {
            current = current.Parent;
        }

        return current;
    }

    private string GetPath()
    {
        var path = string.Empty;
        var current = Parent;

        while (current is not null)
        {
            path = !string.IsNullOrEmpty(path) ? $"{current.Name}/{path}" : $"{current.Name}";
            current = current.Parent;
        }

        return path;
    }

    public override string ToString() => Name;

    public bool Equals(Node? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return NodeId.Equals(other.NodeId);
    }

    public override bool Equals(object? obj) => obj is Node other && other.NodeId == NodeId;
    public override int GetHashCode() => NodeId.GetHashCode();

    public static bool operator ==(Node? left, Node? right) => Equals(left, right);
    public static bool operator !=(Node? left, Node? right) => !Equals(left, right);
}