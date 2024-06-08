namespace AutoSpex.Engine;

public class Run
{
    private readonly Dictionary<Guid, Node> _nodes = [];
    private readonly List<Outcome> _outcomes = [];

    public Run()
    {
    }

    public Run(string name)
    {
        Name = name;
    }

    public Run(Node runNode, Node seedNode)
    {
        RunId = runNode.NodeId;
        Name = runNode.Name;
        AddNodeTree(seedNode);
    }

    public Guid RunId { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = "Run";
    public ResultState Result { get; private set; } = ResultState.None;
    public DateTime RanOn { get; private set; }
    public string RanBy { get; private set; } = string.Empty;
    public IEnumerable<Node> Nodes => _nodes.Values;
    public IEnumerable<Node> Sources => _nodes.Values.Where(n => n.Type == NodeType.Source);
    public IEnumerable<Node> Specs => _nodes.Values.Where(n => n.Type == NodeType.Spec);
    public IEnumerable<Outcome> Outcomes => _outcomes;


    /// <summary>
    /// Adds a node to the Run.
    /// </summary>
    /// <param name="node">The node to be added.</param>
    public void AddNode(Node node)
    {
        AddNodeTree(node);
    }

    /// <summary>
    /// Adds multiple nodes to the Run.
    /// </summary>
    /// <param name="nodes">The nodes to be added.</param>
    /// <exception cref="ArgumentNullException">Thrown when nodes is null.</exception>
    public void AddNodes(IEnumerable<Node> nodes)
    {
        if (nodes is null) throw new ArgumentNullException(nameof(nodes));

        foreach (var node in nodes)
        {
            AddNodeTree(node);
        }
    }

    /// <summary>
    /// Removes a node from the Run.
    /// </summary>
    /// <param name="node">The node to be removed.</param>
    public void RemoveNode(Node node)
    {
        if (node is null)
            throw new ArgumentNullException(nameof(node));

        _nodes.Remove(node.NodeId);
    }

    /// <summary>
    /// Clears all configured nodes from the Run.
    /// </summary>
    public void ClearNodes()
    {
        _nodes.Clear();
    }

    /// <summary>
    /// Adds the provided <see cref="Outcome"/> to the run as a result that it produced.
    /// </summary>
    /// <param name="outcome">The <see cref="Outcome"/> produced from a spec run.</param>
    public void AddOutcome(Outcome outcome)
    {
        if (outcome is null)
            throw new ArgumentNullException(nameof(outcome));

        _outcomes.Add(outcome);
    }
    
    /// <summary>
    /// Adds the provided <see cref="Outcome"/> to the run as a result that it produced.
    /// </summary>
    /// <param name="outcomes">The collection of <see cref="Outcome"/> produced this spec ran.</param>
    public void AddOutcomes(IEnumerable<Outcome> outcomes)
    {
        if (outcomes is null)
            throw new ArgumentNullException(nameof(outcomes));

        foreach (var outcome in outcomes)
        {
            _outcomes.Add(outcome);
        }
    }
    
    /// <summary>
    /// Removes the provided <see cref="Outcome"/> from this Run.
    /// </summary>
    /// <param name="outcome">The <see cref="Outcome"/> produced from a spec run.</param>
    public void RemoveOutcome(Outcome outcome)
    {
        if (outcome is null)
            throw new ArgumentNullException(nameof(outcome));

        _outcomes.Remove(outcome);
    }

    /// <summary>
    /// Clears all outcome result from the Run.
    /// </summary>
    public void ClearOutcomes()
    {
        _outcomes.Clear();
    }

    /// <summary>
    /// Updates the run results using the current outcomes collection.
    /// </summary>
    public void Complete()
    {
        Result = _outcomes.Max(x => x.Result);
        RanOn = DateTime.Now;
        RanBy = Environment.UserName;
    }

    private void AddNodeTree(Node node)
    {
        if (node is null)
            throw new ArgumentNullException(nameof(node), "Can not add null node.");

        //This could be a container or spec (or really anything) but we will call Specs to get only spec nodes to add.
        var nodes = node.Descendents(node.Feature);

        foreach (var descendent in nodes)
        {
            _nodes[descendent.NodeId] = descendent;
        }
    }
}