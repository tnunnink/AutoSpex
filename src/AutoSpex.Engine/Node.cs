using System.Text.Json.Serialization;
using Ardalis.SmartEnum.SystemTextJson;
using JetBrains.Annotations;
using L5Sharp.Core;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Engine;

[PublicAPI]
public class Node : IEquatable<Node>
{
    private readonly List<Node> _nodes = [];

    private Node()
    {
    }

    [JsonConstructor]
    private Node(Guid nodeId, Guid parentId, NodeType type, string name,
        IEnumerable<Node> nodes, Spec spec)
    {
        NodeId = nodeId;
        ParentId = parentId;
        Type = type;
        Name = name;
        Spec = spec;
        _nodes = nodes.ToList();
    }

    /// <summary>
    /// The unique id that identifies this node.
    /// </summary>
    [JsonInclude]
    public Guid NodeId { get; private init; } = Guid.NewGuid();

    /// <summary>
    /// The id of the parent node that this node is contained by.
    /// </summary>
    /// <remarks>
    /// Collection type nodes or orphaned node will have and empty guid id.
    /// </remarks>
    [JsonInclude]
    public Guid ParentId { get; private set; } = Guid.Empty;

    /// <summary>
    /// The reference to the parent node that this node is container by.
    /// </summary>
    [JsonIgnore]
    public Node? Parent { get; private set; }

    /// <summary>
    /// The node type which idendtifies this node as a collection, container, or spec type node. 
    /// </summary>
    [JsonInclude]
    [JsonConverter(typeof(SmartEnumNameConverter<NodeType, int>))]
    public NodeType Type { get; init; } = NodeType.Collection;

    /// <summary>
    /// The configured name of the node. 
    /// </summary>
    [JsonInclude]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Additional comment describing the purpose of this node.
    /// </summary>
    [JsonIgnore]
    public string? Comment { get; set; }

    /// <summary>
    /// The collection of child nodes that this node contains.
    /// </summary>
    [JsonInclude]
    public IEnumerable<Node> Nodes => _nodes;

    /// <summary>
    /// Represents the specification associated with a Node.
    /// </summary>
    [JsonInclude]
    [JsonConverter(typeof(JsonSpecConverter))]
    public Spec Spec { get; private set; } = new();

    /// <summary>
    /// The depth or level of this node the tree heirarchy.
    /// </summary>
    [JsonIgnore]
    public int Depth => GetDepth();

    /// <summary>
    /// The path to this node instance. This does not include this node's name, just the parent names
    /// (as opposed to <see cref="Route"/>).
    /// </summary>
    [JsonIgnore]
    public string Path => GetPath();

    /// <summary>
    /// The full route of this node instance, including this node's name.
    /// (as opposed to <see cref="Path"/>).
    /// </summary>
    [JsonIgnore]
    public string Route => $"{Path}/{Name}".Trim('/');

    /// <summary>
    /// Represents an empty node instance with NodeId set to Guid.Empty.
    /// </summary>
    public static Node Empty => new() { NodeId = Guid.Empty };

    /// <summary>
    /// Creates a new Collection type node.
    /// </summary>
    /// <param name="name">The name of the node (optional). If not provided, the default name will be "New Container".</param>
    /// <returns>The newly created collection node.</returns>
    public static Node NewCollection(string? name = default)
    {
        var node = new Node
        {
            Type = NodeType.Collection,
            Name = name ?? "New Collection"
        };

        return node;
    }

    /// <summary>
    /// Creates a new Container type node.
    /// </summary>
    /// <param name="name">The name of the node (optional). If not provided, the default name will be "New Container".</param>
    /// <returns>The newly created container node.</returns>
    public static Node NewContainer(string? name = default)
    {
        var node = new Node
        {
            Type = NodeType.Container,
            Name = name ?? "New Container"
        };

        return node;
    }

    /// <summary>
    /// Creates a new Spec type node.
    /// </summary>
    /// <param name="name">The name of the node (optional). If not provided, the default name will be "New Spec".</param>
    /// <param name="config">The optional spec config to apply to the <see cref="Spec"/> property upon creation.</param>
    /// <returns>The newly created <see cref="Node"/> instance.</returns>
    public static Node NewSpec(string? name = default, Action<Spec>? config = default)
    {
        var node = new Node
        {
            Type = NodeType.Spec,
            Name = name ?? "New Spec"
        };

        if (config is not null)
            node.Configure(config);

        return node;
    }

    /// <summary>
    /// Adds a Container type node to the current node.
    /// </summary>
    /// <param name="name">The name of the container node (optional). If not provided, the default name will be "New Container".</param>
    /// <returns>The newly added container node.</returns>
    /// <exception cref="InvalidOperationException">Thrown when trying to add a folder to a container node.</exception>
    public Node AddContainer(string? name = default)
    {
        if (Type == NodeType.Spec)
            throw new InvalidOperationException("Can not add a container to a spec node.");

        var node = new Node
        {
            ParentId = NodeId,
            Parent = this,
            Type = NodeType.Container,
            Name = name ?? "New Container",
        };

        _nodes.Add(node);

        return node;
    }

    /// <summary>
    /// Creates and adds a specification node as a child of this container node.
    /// </summary>
    /// <param name="name">The name of the node (optional). If not provided, the default name will be "New Spec".</param>
    /// <param name="config"></param>
    /// <returns>The newly added specification node.</returns>
    /// <exception cref="InvalidOperationException">Thrown when this node is a spec type node.</exception>
    public Node AddSpec(string? name = default, Action<Spec>? config = default)
    {
        if (Type == NodeType.Spec)
            throw new InvalidOperationException("Can not add a spec to another spec node.");

        var node = new Node
        {
            ParentId = NodeId,
            Parent = this,
            Type = NodeType.Spec,
            Name = name ?? "New Spec",
        };

        _nodes.Add(node);

        if (config is not null)
            node.Configure(config);

        return node;
    }

    /// <summary>
    /// Adds a child node to the current node.
    /// </summary>
    /// <param name="node">The node to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="node"/> parameter is null.</exception>
    public void AddNode(Node node)
    {
        ArgumentNullException.ThrowIfNull(node);

        if (_nodes.Contains(node)) return;

        node.Parent?.RemoveNode(node);

        node.Parent = this;
        node.ParentId = NodeId;

        _nodes.Add(node);
    }

    /// <summary>
    /// Removes a child node from the current node.
    /// </summary>
    /// <param name="node">The node to remove.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="node"/> parameter is null.</exception>
    public void RemoveNode(Node node)
    {
        ArgumentNullException.ThrowIfNull(node);

        var result = _nodes.Remove(node);
        if (!result) return;

        node.Parent = default;
        node.ParentId = Guid.Empty;
    }

    /// <summary>
    /// Clears all child nodes from the current node.
    /// </summary>
    /// <remarks>
    /// This method removes all child nodes from the current node.
    /// The child nodes will have their parent node set to null and their parent ID set to Guid.Empty.
    /// This operation does not affect the other properties of the child nodes such as their type, name, or variables.
    /// </remarks>
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
    /// Updates the configured <see cref="Spec"/> for this node using the provided config delegate.
    /// </summary>
    /// <param name="config">The delegate that configures the spec instance.</param>
    /// <remarks>This is mostly a helper for updating spec configs to this node for testing.</remarks>
    public Spec Configure(Action<Spec> config)
    {
        ArgumentNullException.ThrowIfNull(config);
        var spec = new Spec();
        config.Invoke(spec);
        Spec = spec;
        return Spec;
    }

    /// <summary>
    /// Updates the configured <see cref="Spec"/> for this node using the provided spec instance. 
    /// </summary>
    /// <param name="spec">The <see cref="Spec"/> instance to set for this node.
    /// If null will set <see cref="Spec"/> to a default instance.</param>
    public void Configure(Spec? spec)
    {
        Spec = spec ?? new Spec();
    }

    /// <summary>
    /// Creates a duplicate of the current node, including all nested child nodes.
    /// The new node will not have a parent node set. It's up to the user to add the duplicate where needed.
    /// </summary>
    /// <returns>A duplicate of the current node.</returns>
    public Node Duplicate(string? name = default)
    {
        var duplicate = new Node
        {
            ParentId = ParentId,
            Type = Type,
            Name = name ?? Name,
            Spec = Spec.Duplicate()
        };

        foreach (var node in _nodes)
        {
            duplicate.AddNode(node.Duplicate());
        }

        return duplicate;
    }

    /// <summary>
    /// Creates a copy of the current Node with the same NodeId, Type, and Name.
    /// </summary>
    /// <returns>A new Node instance that is a copy of the original Node.</returns>
    public Node Copy()
    {
        var copy = new Node
        {
            NodeId = NodeId,
            Type = Type,
            Name = Name
        };

        return copy;
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
    /// Gets all descendant nodes of this node, inclusing this node itself.
    /// </summary>
    /// <returns>A collection of node that are immediate and/or nested children of this node as well as this node itself.</returns>
    public IEnumerable<Node> DescendantsAndSelf()
    {
        List<Node> nodes = [this];

        foreach (var node in _nodes)
        {
            nodes.Add(node);
            nodes.AddRange(node.Descendants());
        }

        return nodes;
    }

    /// <summary>
    /// Determines whether the node or any of its descendants contain a node with the specified node ID.
    /// </summary>
    /// <param name="nodeId">The ID of the node to search for.</param>
    /// <returns>True if the node or any of its descendants contain a node with the specified ID; otherwise, false.</returns>
    public bool Contains(Guid nodeId)
    {
        foreach (var node in _nodes)
        {
            if (node.NodeId == nodeId) return true;
            if (node.Contains(nodeId)) return true;
        }

        return false;
    }

    /// <summary>
    /// Determines whether the current node is a descendant of the specified node.
    /// </summary>
    /// <param name="node">The node to check if it is an ancestor of the current node.</param>
    /// <returns>True if the current node is a descendant of the specified node; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="node"/> is null.</exception>
    public bool IsDescendantOf(Node node)
    {
        ArgumentNullException.ThrowIfNull(node);

        var current = Parent;

        while (current is not null)
        {
            if (current.NodeId == node.NodeId) return true;
            current = current.Parent;
        }

        return false;
    }

    /// <summary>
    /// Runs all specs configured for this node using the provided L5X content and optional cancellation token.
    /// </summary>
    /// <param name="content">The <see cref="L5X"/> file to run the specs aginst.</param>
    /// <param name="token">The token to cancel the run.</param>
    /// <returns>A <see cref="Task"/> that excutes the specs and returns the flattened <see cref="Verification"/> result.</returns>
    public async Task<Verification> Run(L5X content, CancellationToken token = default)
    {
        return await Spec.RunAsync(content, token);
    }

    /// <inheritdoc />
    public override string ToString() => Name;

    /// <inheritdoc />
    public bool Equals(Node? other)
    {
        if (ReferenceEquals(null, other)) return false;
        return ReferenceEquals(this, other) || NodeId.Equals(other.NodeId);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Node other && other.NodeId == NodeId;

    public static bool operator ==(Node? left, Node? right) => Equals(left, right);
    public static bool operator !=(Node? left, Node? right) => !Equals(left, right);

    /// <inheritdoc />
    public override int GetHashCode() => NodeId.GetHashCode();

    /// <summary>
    /// Gets the depth or level of the node in the tree heirarchy.
    /// </summary>
    private int GetDepth()
    {
        var depth = 0;
        var current = Parent;

        while (current is not null)
        {
            depth++;
            current = current.Parent;
        }

        return depth;
    }

    /// <summary>
    /// Gets the path of the current node by concatenating the names of its ancestors.
    /// </summary>
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
}