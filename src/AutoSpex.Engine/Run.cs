using System.Diagnostics;
using L5Sharp.Core;
using NLog;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Engine;

public class Run(Node node, Source source)
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static readonly SourceCache Cache = SourceCache.Local;
    private readonly Node _node = node ?? throw new ArgumentNullException(nameof(node));

    public Guid RunId { get; } = Guid.NewGuid();
    public NodeInfo Node { get; } = node;
    public Source Source { get; } = source ?? throw new ArgumentNullException(nameof(source));
    public ResultState Result { get; private set; } = ResultState.None;
    public long Duration { get; private set; }
    public IReadOnlyCollection<Verification> Results { get; private set; } = [];
    public IReadOnlyCollection<Run> Runs { get; } = node.Nodes.Select(n => new Run(n, source)).ToArray();

    /// <summary>
    /// Executes the current run, loading the source, processing the node execution, and updating results.
    /// </summary>
    /// <param name="callback">Optional callback executed when a run state changes.</param>
    /// <param name="token">A cancellation token to cancel the run if needed.</param>
    /// <returns>A task representing the asynchronous execution of the run operation.</returns>
    public async Task<RunResult> Execute(Action<Run>? callback = null, CancellationToken token = default)
    {
        Logger.Info("Starting run for {Node} against {Source}.", Node.Name, Source.Location);
        MarkPending(callback);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            //Get or cache the specified source file. Load the L5X content from the cache.
            Logger.Info("Loading source '{Source}'.", Source.Location);
            var cached = await Cache.GetOrAdd(Source, token);
            var target = await cached.OpenAsync(token);
            Logger.Info("Source '{Source}' successfully loaded and cached.", Source.Location);

            //Run this and all descendant nodes (if any) against the loaded content.
            Logger.Info("Running node '{@Node}'.", Node.Name);
            await RunNodeAsync(target, callback, token);

            stopwatch.Stop();

            Logger.Info(
                "{@Node} finished run against {@Source} in {stopwatch.Elapsed} with result: {State}.",
                Node.Name, Source.Name, Duration, Result
            );

            return ProduceRunResult(this, target, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            Logger.Error(ex,
                "An error occurred while processing '{@Source}' against '{@Node}'.",
                Source.Name, Node.Name
            );

            Result = ResultState.Errored;
            return ProduceRunResult(this, TargetInfo.Empty, stopwatch.ElapsedMilliseconds);
        }
    }

    /// <summary>
    /// Retrieves a collection of distinct <see cref="ResultState"/> values from the current run
    /// and all associated descendant runs.
    /// </summary>
    /// <returns>An ordered collection of unique <see cref="ResultState"/> values encountered across all runs.</returns>
    public IEnumerable<ResultState> DistinctResults()
    {
        var states = new HashSet<ResultState> { Result };

        foreach (var run in Runs)
        {
            states.UnionWith(run.DistinctResults());
        }

        return states.OrderBy(s => s.Value);
    }

    /// <summary>
    /// Calculates the total number of runs, including the current and descendant runs,
    /// that match the specified <see cref="ResultState"/>.
    /// </summary>
    /// <param name="state">The <see cref="ResultState"/> to filter and count the runs by.</param>
    /// <returns>The total number of runs that match the specified <see cref="ResultState"/>.</returns>
    public int TotalBy(ResultState state)
    {
        var total = 0;

        if (_node.Type == NodeType.Spec && Result == state)
        {
            total++;
        }

        total += Runs.Sum(n => n.TotalBy(state));
        return total;
    }

    /// <summary>
    /// Asynchronously runs the node with the provided content and optional callback to indicate result state changes
    /// (for consumer/UI updates).
    /// </summary>
    /// <param name="content">The L5X content source to run the node against.</param>
    /// <param name="callback">An optional callback action to be executed for result state changes.</param>
    /// <param name="token">A token to cancel execution of the current task.</param>
    private async Task RunNodeAsync(L5X content, Action<Run>? callback = null, CancellationToken token = default)
    {
        MarkRunning(callback);

        if (_node.Type == NodeType.Spec)
        {
            var stopwatch = Stopwatch.StartNew();
            var verifications = await _node.Spec.RunAsync(content, token);
            stopwatch.Stop();

            MarkComplete(verifications.ToArray(), stopwatch.ElapsedMilliseconds, callback);
            return;
        }

        foreach (var run in Runs)
        {
            token.ThrowIfCancellationRequested();
            await run.RunNodeAsync(content, callback, token);
        }

        MarkComplete(callback);
    }

    /// <summary>
    /// Marks the current run and all descendant runs as pending and resets their state.
    /// </summary>
    /// <param name="callback">An optional callback to invoke for each run marked as pending.</param>
    private void MarkPending(Action<Run>? callback = null)
    {
        Result = ResultState.Pending;
        Duration = 0;
        Results = [];
        callback?.Invoke(this);

        foreach (var run in Runs)
        {
            run.MarkPending(callback);
        }
    }

    /// <summary>
    /// Updates the current state to indicate that the operation is running and optionally invokes a callback with the current run instance.
    /// </summary>
    /// <param name="callback">An optional callback that will be invoked with the current instance when the state is updated to running.</param>
    private void MarkRunning(Action<Run>? callback = null)
    {
        Result = ResultState.Running;
        callback?.Invoke(this);
    }

    /// <summary>
    /// Marks the execution as complete, updates result data, and invokes an optional callback.
    /// </summary>
    /// <param name="verifications">An array of verifications containing the results of the executed evaluations.</param>
    /// <param name="duration">The duration, in milliseconds, of the execution process.</param>
    /// <param name="callback">Optional callback executed after marking the execution as complete.</param>
    private void MarkComplete(Verification[] verifications, long duration, Action<Run>? callback = null)
    {
        Result = ResultState.MaxOrDefault(verifications.Select(e => e.Result).ToList());
        Duration = duration;
        Results = verifications;
        callback?.Invoke(this);
    }

    /// <summary>
    /// Marks the current run as complete by updating its result and duration and notifying through a callback if provided.
    /// </summary>
    /// <param name="callback">An optional callback executed after the run is marked complete.</param>
    private void MarkComplete(Action<Run>? callback = null)
    {
        Result = ResultState.MaxOrDefault(Runs.Select(r => r.Result).ToArray());
        Duration = Runs.Sum(r => r.Duration);
        callback?.Invoke(this);
    }

    /// <summary>
    /// Produces a RunResult object which encapsulates the results of a specific run and its related details.
    /// </summary>
    /// <param name="run">The run instance containing the node, source, and execution context.</param>
    /// <param name="target">The target information loaded during the run's execution.</param>
    /// <param name="duration"></param>
    /// <returns>A RunResult object populated with the run's execution data, including results and metrics.</returns>
    private static RunResult ProduceRunResult(Run run, TargetInfo target, long duration)
    {
        return new RunResult
        {
            Node = run.Node,
            Source = run.Source,
            Target = target,
            Result = run.Result,
            Duration = duration,
            Total = run._node.Descendants(NodeType.Spec).Count(),
            Passed = run.TotalBy(ResultState.Passed),
            Failed = run.TotalBy(ResultState.Failed),
            Errored = run.TotalBy(ResultState.Errored)
        };
    }
}