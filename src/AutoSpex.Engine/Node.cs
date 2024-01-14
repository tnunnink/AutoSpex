using JetBrains.Annotations;

namespace AutoSpex.Engine;

[PublicAPI]
public class Node
{
    private int _ordinal;
    private readonly List<Node> _nodes = [];
    private readonly Dictionary<string, Variable> _variables = [];

    private Node()
    {
        NodeId = Guid.NewGuid();
        ParentId = Guid.Empty;
        Parent = default;
        NodeType = NodeType.Collection;
        Name = string.Empty;
        Depth = 0;
    }

    private Node(Node node)
    {
        NodeId = node.NodeId;
        NodeType = node.NodeType;
        Name = node.Name;
        Depth = node.Depth;
        _ordinal = node._ordinal;
        _variables = node._variables;
        Spec = node.Spec;
    }

    public Guid NodeId { get; private init; }
    public Guid ParentId { get; private set; }
    public Node? Parent { get; private set; }
    public NodeType NodeType { get; private init; }
    public string Name { get; set; }
    public int Depth { get; private set; }
    public int Ordinal => Parent is not null ? Parent._nodes.IndexOf(this) : _ordinal;
    public string Path => GetPath();
    public Node? Collection => GetCollection();
    public IEnumerable<Node> Nodes => _nodes;
    public IEnumerable<Variable> Variables => _variables.Values;
    public Spec? Spec { get; private set; }

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
        var node = new Node
        {
            NodeId = Guid.NewGuid(),
            ParentId = Guid.Empty,
            Parent = default,
            NodeType = NodeType.Spec,
            Name = name ?? "New Spec",
        };

        var spec = new Spec(node);
        config?.Invoke(spec);
        node.Spec = spec;
        return node;
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

    public Node AddSpec(string? name = default)
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
            Depth = Depth + 1,
            Spec = new Spec()
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

    public void InsertNode(Node node, int ordinal)
    {
        if (node is null)
            throw new ArgumentNullException(nameof(node));

        if (!NodeType.CanAdd(node.NodeType))
            throw new ArgumentException($"Can not add a {node.NodeType} to a {NodeType} node.");

        node.Parent?.RemoveNode(node);

        node.Parent = this;
        node.ParentId = NodeId;
        node.Depth = Depth + 1;

        _nodes.Insert(ordinal, node);
    }

    public void RemoveNode(Node node)
    {
        if (node is null)
            throw new ArgumentNullException(nameof(node));

        node.Parent = default;
        node.ParentId = Guid.Empty;

        _nodes.Remove(node);
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

    public void UpdateOrdinal(int ordinal)
    {
        if (Parent is not null)
            throw new ArgumentException("Can not update ordinal for node which belongs to a parent node.");

        _ordinal = ordinal;
    }

    public Variable AddVariable(string name, string value, string? description = default)
    {
        var variable = new Variable(name, value, description);

        if (!_variables.TryAdd(variable.Name, variable))
            throw new ArgumentException($"Variable {variable.Name} already exists for node {Name}.");

        return variable;
    }

    public void AddVariable(Variable variable)
    {
        if (variable is null)
            throw new ArgumentNullException(nameof(variable));

        if (!_variables.TryAdd(variable.Name, variable))
            throw new ArgumentException($"Variable {variable.Name} already exists for node {Name}.");
    }

    public void AddVariables(IEnumerable<Variable> variables)
    {
        if (variables is null)
            throw new ArgumentNullException(nameof(variables));

        foreach (var variable in variables)
            AddVariable(variable);
    }

    public void RemoveVariable(Variable variable)
    {
        if (variable is null)
            throw new ArgumentNullException(nameof(variable));

        _variables.Remove(variable.Name);
    }

    /// <summary>
    /// Finds the first variable with the specified name from the node hierarchy.
    /// </summary>
    /// <param name="name">The name of the variable to find.</param>
    /// <returns>A <see cref="Variable"/> that matches the provided name if found; Otherwise, <c>null</c>.</returns>
    /// <remarks>
    /// This is how variables are resolved from a Spec so that they get the latest value which was persisted
    /// to the database or overriden from a runner.
    /// </remarks>
    public Variable? FindVariable(string name)
    {
        if (_variables.TryGetValue(name, out var variable))
            return variable;

        return Parent?.FindVariable(name);
    }

    public void Configure(Spec spec)
    {
        if (spec is null)
            throw new ArgumentNullException(nameof(spec));

        if (NodeType != NodeType.Spec)
            throw new InvalidOperationException($"Can not apply spec for {NodeType} node.");

        Spec ??= new Spec(this);

        Spec.Element = spec.Element;
        Spec.Settings = spec.Settings;

        Spec.Filters.Clear();
        Spec.Filters.AddRange(spec.Filters);

        Spec.Verifications.Clear();
        Spec.Verifications.AddRange(spec.Filters);
    }

    public void Configure(Action<Spec> config)
    {
        if (config is null)
            throw new ArgumentNullException(nameof(config));

        if (NodeType != NodeType.Spec)
            throw new InvalidOperationException($"Can not apply spec for {NodeType} node.");

        Spec ??= new Spec(this);
        config(Spec);
    }

    public Task<Outcome> Run(Source source)
    {
        if (source is null)
            throw new ArgumentNullException(nameof(source));

        if (Spec is null)
            throw new InvalidOperationException(
                $"Can not perform run on {NodeType} node as it has not spec. To run all child specs use RunAll");

        return Spec.Run(source.L5X);
    }

    public async Task<IEnumerable<Outcome>> RunAll(Source source, IProgress<Outcome>? progress = default)
    {
        if (source is null)
            throw new ArgumentNullException(nameof(source));

        var results = new List<Outcome>();

        if (Spec is not null)
        {
            var result = await Spec.Run(source.L5X);
            progress?.Report(result);
            results.Add(result);
        }

        foreach (var node in Nodes)
        {
            var nested = (await node.RunAll(source, progress)).ToList();
            results.AddRange(nested);
        }

        return results;
    }

    public void Distribute(IEnumerable<KeyValuePair<Guid, string>> overrides)
    {
        if (overrides is null)
            throw new ArgumentNullException(nameof(overrides));

        var variables = GetCollection()?.Descendents()
                            .SelectMany(d => d.Variables)
                            .DistinctBy(v => v.VariableId)
                            .ToDictionary(v => v.VariableId)
                        ?? new Dictionary<Guid, Variable>();

        foreach (var (key, value) in overrides)
        {
            if (!variables.TryGetValue(key, out var variable)) return;
            variable.Value = value;
        }
    }

    public Node OrphanedCopy() => new(this);

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
    public override bool Equals(object? obj) => obj is Node other && other.NodeId == NodeId;
    public override int GetHashCode() => NodeId.GetHashCode();
    public static bool operator ==(Node? left, Node? right) => Equals(left, right);
    public static bool operator !=(Node? left, Node? right) => !Equals(left, right);
}