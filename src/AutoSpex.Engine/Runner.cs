using JetBrains.Annotations;

namespace AutoSpex.Engine;

[PublicAPI]
public class Runner
{
    private readonly Dictionary<Guid, Node> _nodes = [];
    private readonly Dictionary<Guid, string> _overrides = [];

    public Guid RunnerId { get; } = Guid.NewGuid();
    public string Name { get; set; } = "Runner";
    public string Description { get; set; } = string.Empty;
    public IEnumerable<Node> Collections => _nodes.Values.Where(n => n.ParentId == Guid.Empty);

    public IEnumerable<KeyValuePair<Guid, string>> Overrides => _overrides;

    public void AddNode(Node node) => AddNodesFor(node);

    public void AddNodes(IEnumerable<Node> nodes) => nodes.ToList().ForEach(AddNodesFor);

    public void RemoveNode(Node node) => RemoveNodesFor(node);

    public void RemoveNodes(IEnumerable<Node> specs) => specs.ToList().ForEach(RemoveNodesFor);

    public void Override(Guid variableId, string value)
    {
        if (!_overrides.TryAdd(variableId, value))
            _overrides[variableId] = value;
    }

    public async Task<Run> Run(Source source, IProgress<Outcome>? progress = default)
    {
        if (source is null)
            throw new InvalidOperationException("Can not execute runner with null source.");

        var outcomes = new List<Outcome>();

        foreach (var collection in Collections)
        {
            collection.Distribute(_overrides);
            var results = await collection.RunAll(source, progress);
            outcomes.AddRange(results);
        }

        return new Run(this, source, outcomes);
    }

    private void AddNodesFor(Node node)
    {
        if (node is null)
            throw new ArgumentNullException(nameof(node));

        _nodes.TryAdd(node.NodeId, node);

        if (!_nodes.ContainsKey(node.ParentId))
        {
            //the following adds orphan copies of the node's root down to it's parent to ensure consistent tree.
            var ancestors = node.Ancestors().OrderBy(a => a.Depth);
            foreach (var ancestor in ancestors)
            {
                var copy = ancestor.OrphanedCopy();
                
                _nodes.TryAdd(copy.NodeId, copy);
                
                if (_nodes.TryGetValue(ancestor.ParentId, out var parent))
                    parent.AddNode(copy);
            }    
        }
        
        if (_nodes.TryGetValue(node.ParentId, out var runParent))
            runParent.AddNode(node);
    }

    private void RemoveNodesFor(Node node)
    {
        if (node is null)
            throw new ArgumentNullException(nameof(node));

        if (!_nodes.TryGetValue(node.NodeId, out var target)) return;

        _nodes.Remove(target.NodeId);

        foreach (var descendent in target.Descendents())
        {
            _nodes.Remove(descendent.NodeId);
        }
    }
}