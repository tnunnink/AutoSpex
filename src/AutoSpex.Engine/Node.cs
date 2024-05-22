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
        Type = NodeType.Container;
        Name = string.Empty;
        Depth = 0;
    }

    public Guid NodeId { get; private init; }
    public Guid ParentId { get; private set; }
    public Node? Parent { get; private set; }
    public NodeType Type { get; private init; }
    public NodeType Feature => Root.Type;
    public string Name { get; set; }
    public int Depth { get; private set; }
    public string Path => GetPath();
    public Node Base => GetBaseNode();
    public Node Root => GetRootNode();
    public IEnumerable<Node> Nodes => _nodes;

    public static Node NewNode(NodeType type, string? name = default)
    {
        return new Node
        {
            NodeId = Guid.NewGuid(),
            ParentId = Guid.Empty,
            Parent = default,
            Type = type,
            Name = name ?? $"New {type}"
        };
    }

    public static Node NewContainer(string? name = default)
    {
        return new Node
        {
            NodeId = Guid.NewGuid(),
            ParentId = Guid.Empty,
            Parent = default,
            Type = NodeType.Container,
            Name = name ?? "New Container"
        };
    }

    public static Node NewSpec(string? name = default)
    {
        return new Node
        {
            NodeId = Guid.NewGuid(),
            ParentId = Guid.Empty,
            Parent = default,
            Type = NodeType.Spec,
            Name = name ?? "New Spec"
        };
    }

    public static Node NewSource(string? name = default)
    {
        return new Node
        {
            NodeId = Guid.NewGuid(),
            ParentId = Guid.Empty,
            Parent = default,
            Type = NodeType.Source,
            Name = name ?? "New Source"
        };
    }

    public static Node NewRun(string? name = default)
    {
        return new Node
        {
            NodeId = Guid.NewGuid(),
            ParentId = Guid.Empty,
            Parent = default,
            Type = NodeType.Run,
            Name = name ?? "New Run"
        };
    }

    public Node AddContainer(string? name = default)
    {
        if (Type != NodeType.Container)
            throw new ArgumentException($"Can not add a folder to a {Type} node.");

        var node = new Node
        {
            NodeId = Guid.NewGuid(),
            ParentId = NodeId,
            Parent = this,
            Type = NodeType.Container,
            Name = name ?? "New Folder",
            Depth = Depth + 1
        };

        _nodes.Add(node);
        return node;
    }

    public Node AddSpec(string? name = default)
    {
        if (Type != NodeType.Container)
            throw new ArgumentException($"Can not add content to a {Type} node.");

        var node = new Node
        {
            NodeId = Guid.NewGuid(),
            ParentId = NodeId,
            Parent = this,
            Type = NodeType.Spec,
            Name = name ?? "New Spec",
            Depth = Depth + 1
        };

        _nodes.Add(node);
        return node;
    }

    public Node AddSource(string? name = default)
    {
        if (Type != NodeType.Container)
            throw new ArgumentException($"Can not add content to a {Type} node.");

        var node = new Node
        {
            NodeId = Guid.NewGuid(),
            ParentId = NodeId,
            Parent = this,
            Type = NodeType.Source,
            Name = name ?? "New Source",
            Depth = Depth + 1
        };

        _nodes.Add(node);
        return node;
    }

    public Node AddRun(string? name = default)
    {
        if (Type != NodeType.Container)
            throw new ArgumentException($"Can not add content to a {Type} node.");

        var node = new Node
        {
            NodeId = Guid.NewGuid(),
            ParentId = NodeId,
            Parent = this,
            Type = NodeType.Run,
            Name = name ?? "New Run",
            Depth = Depth + 1
        };

        _nodes.Add(node);
        return node;
    }

    public void AddNode(Node node)
    {
        if (node is null)
            throw new ArgumentNullException(nameof(node));

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

        var result = _nodes.Remove(node);
        if (!result) return;

        node.Parent = default;
        node.ParentId = Guid.Empty;
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
        //The node with a null parent should be the root feature node which we don't want to include.
        while (current?.Parent is not null)
        {
            nodes.Add(current);
            current = current.Parent;
        }

        return nodes.OrderBy(n => n.Depth);
    }

    public IEnumerable<Node> AncestorsAndSelf()
    {
        var nodes = new List<Node>();

        var current = this;
        //The node with a null parent should be the root feature node which we don't want to include.
        while (current?.Parent is not null)
        {
            nodes.Add(current);
            current = current.Parent;
        }

        return nodes.OrderBy(n => n.Depth);
    }

    /// <summary>
    /// Gets all descendant nodes of this node.
    /// </summary>
    /// <returns>A collection of <see cref="Node"/> that are immediate and/or nested children of this node.</returns>
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
    /// Gets all descendent spec nodes of thid node, including this node if it is a spec type node.
    /// </summary>
    /// <returns>A collection of<see cref="Node"/>  type <see cref="NodeType.Spec"/> that are immediate and/or \
    /// nested children of this node.</returns>
    public IEnumerable<Node> Specs()
    {
        List<Node> nodes = [];

        if (Type == NodeType.Spec)
            nodes.Add(this);

        nodes.AddRange(_nodes.SelectMany(n => n.Specs()));

        return nodes;
    }

    /// <summary>
    /// Gets the base node containing this node. This is not the root node but the node immediately after this node.
    /// </summary>
    private Node GetBaseNode()
    {
        var current = this;

        //just look to the parent's parent, so we can stop a node before that.
        while (current.Parent?.Parent is not null)
        {
            current = current.Parent;
        }

        return current;
    }

    /// <summary>
    /// All nodes should have a root feature node which is seeded into the database for each project.
    /// Each root node will have a null parent, and this method will travers the parents until this node is reached, and
    /// return it.
    /// </summary>
    private Node GetRootNode()
    {
        var current = this;

        while (current.Parent is not null)
        {
            current = current.Parent;
        }

        return current;
    }

    /// <summary>
    /// Gets the path of the current node by concatenating the names of its ancestors.
    /// </summary>
    /// <returns>The path of the current node as a string.</returns>
    private string GetPath()
    {
        var path = string.Empty;
        var current = Parent;

        while (current?.Parent is not null)
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
        return ReferenceEquals(this, other) || NodeId.Equals(other.NodeId);
    }

    public override bool Equals(object? obj) => obj is Node other && other.NodeId == NodeId;
    public override int GetHashCode() => NodeId.GetHashCode();
    public static bool operator ==(Node? left, Node? right) => Equals(left, right);
    public static bool operator !=(Node? left, Node? right) => !Equals(left, right);
}