using JetBrains.Annotations;

namespace AutoSpex.Engine;

[PublicAPI]
public class Node : IEquatable<Node>
{
    private readonly List<Node> _nodes = [];
    private readonly Dictionary<string, Variable> _variables = [];

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
    public string Name { get; set; }
    public string? Comment { get; set; }
    public int Depth { get; private set; }
    public string Path => GetPath();
    public string Route => $"{Path}/{Name}".Trim('/');
    public IEnumerable<Node> Nodes => _nodes;
    public IEnumerable<Variable> Variables => _variables.Values;

    public static Node New(NodeType type)
    {
        return new Node
        {
            NodeId = Guid.NewGuid(),
            ParentId = Guid.Empty,
            Parent = default,
            Type = type,
            Name = $"New {type}"
        };
    }

    public static Node New(Guid id, string name, NodeType type)
    {
        return new Node
        {
            NodeId = id,
            ParentId = Guid.Empty,
            Parent = default,
            Type = type,
            Name = name
        };
    }

    public static Node NewCollection(string? name = default)
    {
        return new Node
        {
            NodeId = Guid.NewGuid(),
            ParentId = Guid.Empty,
            Parent = default,
            Type = NodeType.Collection,
            Name = name ?? "New Collection"
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

    public Node AddContainer(string? name = default)
    {
        if (Type == NodeType.Spec)
            throw new InvalidOperationException("Can not add a folder to a spec node.");

        var node = new Node
        {
            NodeId = Guid.NewGuid(),
            ParentId = NodeId,
            Parent = this,
            Type = NodeType.Container,
            Name = name ?? "New Container",
            Depth = Depth + 1
        };

        _nodes.Add(node);
        return node;
    }

    public Node AddSpec(string? name = default)
    {
        if (Type == NodeType.Spec)
            throw new InvalidOperationException("Can not add a folder to a spec node.");

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

    /// <summary>
    /// Gets all ancestor nodes of this node.
    /// </summary>
    /// <returns>A collection of <see cref="Node"/> that are immediate and/or nested parents of this node.</returns>
    public IEnumerable<Node> Ancestors()
    {
        var nodes = new List<Node>();

        var current = Parent;

        while (current is not null)
        {
            nodes.Add(current);
            current = current.Parent;
        }

        return nodes.OrderBy(n => n.Depth);
    }

    /// <summary>
    /// Gets all ancestor nodes of this node, including itself.
    /// </summary>
    /// <returns>
    /// A collection of <see cref="Node"/> objects that are immediate and/or nested parents of this node,
    /// including itself.
    /// </returns>
    public IEnumerable<Node> AncestorsAndSelf()
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

    /// <summary>
    /// Gets all descendant nodes of this node.
    /// </summary>
    /// <returns>A collection of <see cref="Node"/> that are immediate and/or nested children of this node.</returns>
    public IEnumerable<Node> Descendants()
    {
        List<Node> nodes = [];

        foreach (var node in _nodes)
        {
            nodes.Add(node);
            nodes.AddRange(node.Descendants());
        }

        return nodes;
    }

    /// <summary>
    /// Gets all descendant nodes of this node.
    /// </summary>
    /// <returns>A collection of <see cref="Node"/> that are immediate and/or nested children of this node.</returns>
    public IEnumerable<Node> Descendants(NodeType type)
    {
        List<Node> nodes = [];

        if (Type == type) nodes.Add(this);
        nodes.AddRange(_nodes.SelectMany(n => n.Descendants(type)));

        return nodes;
    }

    /// <summary>
    /// Adds the provided variable to this node and sets the NodeId of the variable to match.
    /// </summary>
    /// <param name="variable">The <see cref="Variable"/> instance to add.</param>
    public void AddVariable(Variable variable)
    {
        if (variable is null)
            throw new ArgumentNullException(nameof(variable));

        if (variable.NodeId != NodeId)
            throw new ArgumentException("Variable does not have valid node id.");

        _variables[variable.Name] = variable;
    }

    /// <summary>
    /// Resolves the provided reference to a named variable by traversing up this node tree until it finds the first
    /// mating variable name, and returns that variable's value. If not variable is found, meaning unresolvable, then
    /// this method returns null.
    /// </summary>
    /// <param name="reference">The reference to a scoped variable to resolve.</param>
    /// <returns>The value of the referenced variable if found, otherwise, <c>null</c></returns>
    public object? Resolve(Reference reference)
    {
        return _variables.TryGetValue(reference.Name, out var variable) ? variable.Value : Parent?.Resolve(reference);
    }

    /// <summary>
    /// Gets the path of the current node by concatenating the names of its ancestors.
    /// </summary>
    /// <returns>The path of the current node as a string.</returns>
    private string GetPath()
    {
        var path = string.Empty;
        var current = Parent;

        while (current is not null)
        {
            path = !string.IsNullOrEmpty(path) ? string.Concat(current.Name, "/", path) : current.Name;
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