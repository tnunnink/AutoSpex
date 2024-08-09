namespace AutoSpex.Engine;

/// <summary>
/// A <see cref="Run"/> represents a set of specs and source which should be executed together and produce a set of
/// outcomes. 
/// </summary>
public class Run
{
    private readonly Dictionary<Guid, Outcome> _outcomes = [];

    public Run()
    {
    }

    public Run(Environment environment, Node? seed = default)
    {
        Environment = environment;

        if (seed is not null)
            AddNodeTree(seed);
    }

    public Guid RunId { get; private set; } = Guid.NewGuid();
    public string Name => $"{Environment.Name} - Run Results";
    public Environment Environment { get; set; } = Environment.Default;
    public ResultState Result { get; private set; } = ResultState.None;
    public DateTime RanOn { get; private set; }
    public string RanBy { get; private set; } = string.Empty;
    public IEnumerable<Outcome> Outcomes => _outcomes.Values;

    /// <summary>
    /// Adds all descendant spec nodes to the Run <see cref="Outcomes"/> collection.
    /// </summary>
    /// <param name="node">The node to be added.</param>
    public void AddNode(Node node) => AddNodeTree(node);

    /// <summary>
    /// Adds multiple nodes to the Run <see cref="Outcomes"/>. collection.
    /// </summary>
    /// <param name="nodes">The nodes to be added.</param>
    public void AddNodes(IEnumerable<Node> nodes) => nodes.ToList().ForEach(AddNodeTree);

    /// <summary>
    /// Removes a single spec node to this Run <see cref="Outcomes"/> collection.
    /// </summary>
    /// <param name="node">The node to be removed.</param>
    /// <remarks>If the node is not in the run or not a spec node then nothing happens.</remarks>
    public void RemoveNode(Node node) => RemoveNodeInternal(node);

    /// <summary>
    /// Removes the collection of spec nodes from this Run from the <see cref="Outcomes"/> collection.
    /// </summary>
    /// <param name="nodes">The nodes to be removed.</param>
    public void RemoveNodes(IEnumerable<Node> nodes) => nodes.ToList().ForEach(RemoveNodeInternal);

    /// <summary>
    /// Clears all configured outcomes from the run.
    /// </summary>
    public void Clear() => _outcomes.Clear();

    /// <summary>
    /// Loads the collection of outcomes into the run.
    /// </summary>
    /// <param name="outcomes">The collection of outcomes to load.</param>
    /// <remarks>This is for the database to be able to add the persisted collection of outcomes.</remarks>
    public void Load(IEnumerable<Outcome>? outcomes)
    {
        outcomes?.ToList().ForEach(AddOutcomeInternal);
    }

    /// <summary>
    /// Executes the provided <see cref="Spec"/> objects against the configured Environment for this Run.
    /// </summary>
    /// <param name="specs">The specifications to run.</param>
    /// <param name="running">A callback prior to running each spec.
    /// This action give the outcome instance that is about to be run so that the UI can respond and update the result state.</param>
    /// <param name="complete">A callback after the spec has been run to completion.
    /// This action provides the outcome instance that ran so that the UI can respond and update the result state and result data.</param>
    /// <param name="token">The cancellation token to cancel the execution of this run.</param>
    public async Task ExecuteAsync(IEnumerable<Spec> specs,
        Action<Outcome>? running = default, Action<Outcome>? complete = default, CancellationToken token = default)
    {
        var sources = Environment.Sources.ToList();

        foreach (var spec in specs)
        {
            token.ThrowIfCancellationRequested();
            if (!_outcomes.TryGetValue(spec.SpecId, out var outcome)) continue;

            running?.Invoke(outcome);

            var result = await spec.RunAllAsync(sources, token);
            outcome.Update(result);

            complete?.Invoke(outcome);
        }

        Result = ResultState.MaxOrDefault(_outcomes.Select(o => o.Value.Result).ToList());
        RanOn = DateTime.Now;
        RanBy = System.Environment.UserName;

        //After running, I want to try and release references to the L5X files that were loaded as we ran.
        foreach (var source in sources)
            source.Release();
    }

    /// <summary>
    /// Executes the provided <see cref="Spec"/> objects against the configured Environment for this Run.
    /// </summary>
    /// <param name="specs">The specifications to run.</param>
    public void Execute(IEnumerable<Spec> specs)
    {
        var sources = Environment.Sources.ToList();
        //todo if the environment contains override we could resolve those here.

        foreach (var spec in specs)
        {
            if (!_outcomes.TryGetValue(spec.SpecId, out var outcome)) continue;

            var result = spec.RunAll(sources);
            outcome.Update(result);
        }

        Result = ResultState.MaxOrDefault(_outcomes.Select(o => o.Value.Result).ToList());
        RanOn = DateTime.Now;
        RanBy = System.Environment.UserName;

        foreach (var source in sources)
            source.Release();
    }

    /// <summary>
    /// Finds all descendant spec nodes and generates a collection of new <see cref="Spec"/> objects to be
    /// added to this <see cref="Run"/>. These spec objects won't have a configuration, so it would be expected that
    /// the caller first loads the specs before running. This is for the interface to allow dragging nodes to add entire
    /// tree of specs.
    /// </summary>
    private void AddNodeTree(Node node)
    {
        ArgumentNullException.ThrowIfNull(node);

        var nodes = node.Descendants(NodeType.Spec);

        foreach (var descendent in nodes)
        {
            var outcome = new Outcome(descendent);
            AddOutcomeInternal(outcome);
        }
    }

    /// <summary>
    /// Removes the specified node by id.
    /// </summary>
    private void RemoveNodeInternal(Node node)
    {
        ArgumentNullException.ThrowIfNull(node);
        _outcomes.Remove(node.NodeId);
    }

    /// <summary>
    /// Adds or updates the provided outcome for this run.
    /// </summary>
    private void AddOutcomeInternal(Outcome outcome)
    {
        ArgumentNullException.ThrowIfNull(outcome);
        _outcomes.TryAdd(outcome.SpecId, outcome);
    }
}