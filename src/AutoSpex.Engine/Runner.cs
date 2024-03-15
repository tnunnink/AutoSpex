using JetBrains.Annotations;

namespace AutoSpex.Engine;

[PublicAPI]
public class Runner
{
    private readonly Dictionary<Guid, Node> _nodes = [];
    private readonly Dictionary<Guid, string> _overrides = [];

    public Guid RunnerId { get; } = Guid.NewGuid();
    public string Name { get; set; } = "Runner";
    public Source? Source { get; set; }
    public IEnumerable<Node> Collections => _nodes.Values.Where(n => n.NodeType == NodeType.Collection);
    public IEnumerable<Node> Specs => _nodes.Values.Where(n => n.NodeType == NodeType.Spec);
    public IEnumerable<KeyValuePair<Guid, string>> Overrides => _overrides;

    /// <summary>
    /// Executes the runner with the given input source.
    /// </summary>
    /// <param name="source">The input source to execute the runner.</param>
    /// <param name="progress">The progress object for tracking the execution progress. (optional)</param>
    /// <returns>The Run object that encapsulates the execution outcome.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the input source is null.</exception>
    public async Task<Run> Run(Source source, IProgress<Outcome>? progress = default)
    {
        if (source is null)
            throw new InvalidOperationException("Can not execute runner with no input source.");

        var outcomes = new List<Outcome>();

        foreach (var collection in Collections)
        {
            collection.Distribute(_overrides);
            var results = await collection.RunAll(source, progress);
            outcomes.AddRange(results);
        }

        return new Run(this, source, outcomes);
    }

    /// <summary>
    /// Adds a node to the Runner.
    /// </summary>
    /// <param name="node">The node to be added.</param>
    /// <exception cref="ArgumentNullException">Thrown when the node is null.</exception>
    /// <remarks>
    /// This will add just this spec if it is a spec node. If a collection of folder it will add all
    /// descendent spec nodes.
    /// </remarks>
    public void AddNode(Node node)
    {
        if (node is null)
            throw new ArgumentNullException(nameof(node));

        if (node.NodeType == NodeType.Spec)
        {
            _nodes.TryAdd(node.NodeId, node);
            return;
        }

        var specs = node.Descendents().Where(n => n.NodeType == NodeType.Spec);

        foreach (var spec in specs)
        {
            _nodes.TryAdd(spec.NodeId, spec);
        }
    }

    /// <summary>
    /// Adds a collection of nodes to the Runner.
    /// </summary>
    /// <param name="nodes">The collection of nodes to add.</param>
    /// <remarks>
    /// This will add just this spec if it is a spec node. If a collection of folder it will add all
    /// descendent spec nodes.
    /// </remarks>
    public void AddNodes(IEnumerable<Node> nodes) => nodes.ToList().ForEach(AddNode);

    /// <summary>
    /// Removes the specified node from the Runner.
    /// </summary>
    /// <param name="node">The node to be removed.</param>
    /// <exception cref="ArgumentNullException">Thrown when the 'node' parameter is null.</exception>
    public void RemoveNode(Node node)
    {
        if (node is null)
            throw new ArgumentNullException(nameof(node));

        _nodes.Remove(node.NodeId);
    }

    /// <summary>
    /// Removes the specified nodes from the Runner.
    /// </summary>
    /// <param name="specs">The nodes to be removed.</param>
    public void RemoveNodes(IEnumerable<Node> specs) => specs.ToList().ForEach(RemoveNode);

    /// <summary>
    /// Clears all node objects within the runner.
    /// </summary>
    public void ClearNodes() => _nodes.Clear();

    /// <summary>
    /// Overrides the value of a variable for the runner.
    /// </summary>
    /// <param name="variableId">The ID of the variable to override.</param>
    /// <param name="value">The new value of the variable.</param>
    public void Override(Guid variableId, string value)
    {
        if (!_overrides.TryAdd(variableId, value))
            _overrides[variableId] = value;
    }
}