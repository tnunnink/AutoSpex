namespace AutoSpex.Engine;

/// <summary>
/// A <see cref="Run"/> represents a set of specs and source which should be executed together and produce a set of
/// outcomes. This object allows adding nodes (either source of spec or their containers) to configure the run. It also
/// contains the actual <see cref="Outcomes"/> that are the result of the run. These data can be persisted and retrieved
/// from the database to be run again. When a run is executed, it is up to the caller to provide the fully loaded
/// source and spec data objects in order to completely process the results. 
/// </summary>
public class Run
{
    private readonly Dictionary<ValueTuple<Guid, Guid>, Outcome> _outcomes = [];

    /// <summary>
    /// Creates a new <see cref="Run"/> configuration.
    /// </summary>
    public Run()
    {
    }

    /// <summary>
    /// Creates a new <see cref="Run"/> with the provided name.
    /// </summary>
    /// <param name="name">The name of the run.</param>
    public Run(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Creates a new <see cref="Run"/> with the provided run node and optional seed node.
    /// </summary>
    /// <param name="run">A <see cref="Node"/> representing the run object this configuration applies to.</param>
    /// <param name="seed">A <see cref="Node"/> representing the spec or source (or container of) node(s) to seed the run with.</param>
    public Run(Node run, Node? seed = default)
    {
        ArgumentNullException.ThrowIfNull(run);

        RunId = run.NodeId;
        Name = run.Name;

        if (seed is not null)
            AddNodeTree(seed);
    }

    public Guid RunId { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = "Run";
    public ResultState Result => _outcomes.Count > 0 ? _outcomes.Values.Max(x => x.Result) : ResultState.None;
    public DateTime RanOn { get; private set; }
    public string RanBy { get; private set; } = string.Empty;
    public IEnumerable<Outcome> Outcomes => _outcomes.Values;
    public IEnumerable<Node> Nodes => GetNodes();
    public IEnumerable<Node> Sources => GetNodes(NodeType.Source);
    public IEnumerable<Node> Specs => GetNodes(NodeType.Spec);


    /// <summary>
    /// Adds the provided <see cref="Outcome"/> to the run as a result that it produced.
    /// </summary>
    /// <param name="outcome">The <see cref="Outcome"/> produced from a spec run.</param>
    public void AddOutcome(Outcome outcome)
    {
        UpsertOutcomes([outcome]);
    }

    /// <summary>
    /// Adds the provided <see cref="Outcome"/> to the run as a result that it produced.
    /// </summary>
    /// <param name="outcomes">The collection of <see cref="Outcome"/> produced this spec ran.</param>
    public void AddOutcomes(IEnumerable<Outcome> outcomes)
    {
        UpsertOutcomes(outcomes);
    }

    /// <summary>
    /// Removes the specified outcome from the run.
    /// </summary>
    /// <param name="outcome">The outcome to be removed.</param>
    public void RemoveOutcome(Outcome outcome)
    {
        DeleteOutcomes([outcome]);
    }

    /// <summary>
    /// Removes the specified collection of outcomes from the run.
    /// </summary>
    /// <param name="outcomes">The collection of outcomes to be removed.</param>
    public void RemoveOutcome(IEnumerable<Outcome> outcomes)
    {
        DeleteOutcomes(outcomes);
    }

    /// <summary>
    /// Adds a node and all its descendants of the target feature to the Run by generating the set of outcomes to be
    /// executed for the target node.
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
    public void AddNodes(IEnumerable<Node> nodes)
    {
        if (nodes is null) throw new ArgumentNullException(nameof(nodes));

        foreach (var node in nodes)
        {
            AddNodeTree(node);
        }
    }

    /// <summary>
    /// Clears all configured <see cref="Outcomes"/> from the Run.
    /// </summary>
    public void Clear() => _outcomes.Clear();

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
        var lookup = _outcomes.Values
            .Where(x => x.Spec is not null && x.Source is not null && x.Source.NodeId == source.SourceId)
            .ToDictionary(s => s.Spec!.NodeId);

        foreach (var spec in specs)
        {
            if (!lookup.TryGetValue(spec.SpecId, out var outcome)) continue;
            preRun?.Invoke(outcome);
            //await Task.Delay(5000, token);
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
    private void AddNodeTree(Node node)
    {
        ArgumentNullException.ThrowIfNull(node);

        //Get descendant will return the nodes wee are interested in (either spec or source).
        var nodes = node.Descendents(node.Feature);

        foreach (var descendent in nodes)
        {
            var outcomes = GenerateOutcomes(descendent);
            UpsertOutcomes(outcomes);
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
        //Get the sources/specs that are opposite the provided feature node.
        var others = GetNodes().Where(n => n.Feature != node.Feature).ToList();

        switch (others.Count)
        {
            //No others configured - just add a single outcome per node
            case 0:
                return [new Outcome(node)];
            //Others exist and this is a spec node and all outcomes already have spec configured, return new set for each source.
            case > 0 when node.Type == NodeType.Spec && Outcomes.All(x => x.Spec is not null):
                return others.Select(s => new Outcome(node, s));
            //Others exist and this is a source node and all outcomes already have source configured, return new set for each spec.
            case > 0 when node.Type == NodeType.Source && Outcomes.All(x => x.Source is not null):
                return others.Select(s => new Outcome(s, node));
            //Others exist and this is a spec node and all outcomes have null spec, just configure current outcomes.
            case > 0 when node.Type == NodeType.Spec && Outcomes.All(x => x.Spec is null):
                var specConfigured = _outcomes.Values.Select(x => x.ConfigureSpec(node)).ToList();
                _outcomes.Clear();
                return specConfigured;
            //Others exist and this is a source node and all outcomes have null source, just configure current outcomes.
            case > 0 when node.Type == NodeType.Source && Outcomes.All(x => x.Source is null):
                var sourceConfigured = _outcomes.Values.Select(x => x.ConfigureSource(node)).ToList();
                _outcomes.Clear();
                return sourceConfigured;
        }

        //If we get to here we didn't produce new outcomes so return nothing.
        return Enumerable.Empty<Outcome>();
    }

    /// <summary>
    /// Generates a collection of virtual nodes as defined by the configured outcomes for this run. Will return all
    /// unless a feature <see cref="NodeType"/> is provided in which case it will only return the nodes for the
    /// corresponding type.
    /// </summary>
    private IEnumerable<Node> GetNodes(NodeType? feature = default)
    {
        var nodes = new Dictionary<Guid, Node>();

        foreach (var outcome in _outcomes.Values)
        {
            if (outcome.Spec is not null && (feature is null || feature == NodeType.Spec))
            {
                nodes.TryAdd(outcome.Spec.NodeId, outcome.Spec);
            }

            // ReSharper disable once InvertIf
            if (outcome.Source is not null && (feature is null || feature == NodeType.Source))
            {
                nodes.TryAdd(outcome.Source.NodeId, outcome.Source);
            }
        }

        return nodes.Values;
    }

    /// <summary>
    /// Adds/updates <see cref="Outcomes"/> with the provided collection by iterating and generating the required
    /// composite key.
    /// </summary>
    private void UpsertOutcomes(IEnumerable<Outcome> outcomes)
    {
        ArgumentNullException.ThrowIfNull(outcomes);

        foreach (var outcome in outcomes)
        {
            ArgumentNullException.ThrowIfNull(outcome);
            var key = GetKey(outcome);
            _outcomes[key] = outcome;
        }
    }

    /// <summary>
    /// Removes all <see cref="Outcomes"/> that match the provided collection of outcome object.
    /// </summary>
    private void DeleteOutcomes(IEnumerable<Outcome> outcomes)
    {
        ArgumentNullException.ThrowIfNull(outcomes);

        foreach (var outcome in outcomes)
        {
            ArgumentNullException.ThrowIfNull(outcome);
            var key = GetKey(outcome);
            _outcomes.Remove(key);
        }
    }

    /// <summary>
    /// Generates the composite outcome key used to uniquely identify the outcome (as a combination of spec/source).
    /// </summary>
    private static (Guid, Guid) GetKey(Outcome outcome) =>
        new(outcome.Spec?.NodeId ?? Guid.Empty, outcome.Source?.NodeId ?? Guid.Empty);
}