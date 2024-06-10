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
    public ResultState Result => _outcomes.Max(x => x.Result);
    public DateTime RanOn { get; private set; }
    public string RanBy { get; private set; } = string.Empty;
    public IEnumerable<Node> Nodes => _nodes.Values;
    public IEnumerable<Node> Sources => _nodes.Values.Where(n => n.Type == NodeType.Source);
    public IEnumerable<Node> Specs => _nodes.Values.Where(n => n.Type == NodeType.Spec);
    public IEnumerable<Outcome> Outcomes => _outcomes;


    /// <summary>
    /// Adds a node and all its descendants of the target feature to the Run.
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
        _outcomes.RemoveAll(x => x.SpecId == node.NodeId || x.SourceId == node.NodeId);
    }

    /// <summary>
    /// Adds the provided <see cref="Outcome"/> to the run as a result that it produced.
    /// </summary>
    /// <param name="outcome">The <see cref="Outcome"/> produced from a spec run.</param>
    public void AddOutcome(Outcome outcome)
    {
        if (outcome is null)
            throw new ArgumentNullException(nameof(outcome));

        AddOutcomeNodes(outcome);
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
            AddOutcomeNodes(outcome);
        }
    }

    /// <summary>
    /// Clears all configured <see cref="Nodes"/> and <see cref="Outcomes"/> from the Run.
    /// </summary>
    public void Clear()
    {
        _nodes.Clear();
        _outcomes.Clear();
    }

    /// <summary>
    /// Runs a <see cref="Source"/> agains the provided <see cref="Spec"/> objects and updates the result of each
    /// outcome configured for this run. If either source or specs are not configured for this run, then no result will
    /// be posted as there should be no corresponding Outcome object. 
    /// </summary>
    /// <param name="source">The <see cref="Source"/> containing the L5X content to process.</param>
    /// <param name="specs">The collection of <see cref="Spec"/> configurations to run against the provided source.</param>
    /// <param name="preRun">An optional callback action which is called right before executing the outcome.</param>
    /// <param name="postRun">An optional callback action which is called right after executing the outcome.</param>
    /// <param name="token">The cancellation token to cancel the current execution.</param>
    
    public async Task Execute(Source source, IEnumerable<Spec> specs,
        Action<Outcome>? preRun = default, Action<Outcome>? postRun = default, CancellationToken token = default)
    {
        var lookup = _outcomes.Where(x => x.SourceId == source.SourceId).ToDictionary(s => s.SpecId);

        foreach (var spec in specs)
        {
            if (!lookup.TryGetValue(spec.SpecId, out var outcome)) continue;
            preRun?.Invoke(outcome);
            await outcome.Run(source, spec, token);
            postRun?.Invoke(outcome);
        }

        //Update local properties to indicate when this run was last ran.
        RanOn = DateTime.Now;
        RanBy = Environment.UserName;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="outcome"></param>
    /// <exception cref="ArgumentNullException"></exception>
    private void AddOutcomeNodes(Outcome outcome)
    {
        if (outcome is null)
            throw new ArgumentNullException(nameof(outcome), "Can not add null outcome.");

        var spec = Node.Create(outcome.SpecId, NodeType.Spec, outcome.SpecName);
        var source = Node.Create(outcome.SourceId, NodeType.Source, outcome.SourceName);

        //If either node gets added to the run as in one of them didn't already exist,
        //we can add the corresponding outcome instance.
        if (_nodes.TryAdd(spec.NodeId, spec) || _nodes.TryAdd(source.NodeId, source))
        {
            _outcomes.Add(outcome);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void AddNodeTree(Node node)
    {
        if (node is null)
            throw new ArgumentNullException(nameof(node), "Can not add null node.");

        //Get descendant will return the nodes wee are interested in (either spec or source).
        var nodes = node.Descendents(node.Feature);

        foreach (var descendent in nodes)
        {
            if (_nodes.TryAdd(descendent.NodeId, descendent))
            {
                _outcomes.AddRange(GenerateOutcomes(descendent));
                continue;
            }

            _nodes[descendent.NodeId] = descendent;
        }
    }

    /// <summary>
    /// Generates a collection of empty <see cref="Outcome"/> instances to be added to this run. For each spec we want
    /// to add an outcome for each configured source. And for each source we want to add an outcome for each configured spec.
    /// In other words, we want a Cartesian product of the set of all source/spec nodes because we want to run each spec
    /// for each source. 
    /// </summary>
    private IEnumerable<Outcome> GenerateOutcomes(Node node)
    {
        var others = _nodes.Values.Where(n => n.Feature != node.Feature).ToList();

        if (node.Feature == NodeType.Spec)
            return others.Select(n => Outcome.Empty(node, n));

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (node.Feature == NodeType.Source)
            return others.Select(n => Outcome.Empty(n, node));

        return Enumerable.Empty<Outcome>();
    }
}