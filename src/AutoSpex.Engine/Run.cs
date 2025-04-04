using System.Diagnostics;
using NLog;

namespace AutoSpex.Engine;

public class Run(Node node, Source source)
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static readonly SourceCache Cache = SourceCache.Local;
    private int _specTotal;
    private int _specCount;

    public Node Node { get; } = node ?? throw new ArgumentNullException(nameof(node));
    public Source Source { get; } = source ?? throw new ArgumentNullException(nameof(source));
    public ResultState State { get; private set; } = ResultState.None;
    public float Progress { get; private set; }
    public DateTime RanOn { get; private set; }
    public string? RanBy { get; private set; }
    public long Duration { get; private set; }

    /// <summary>
    /// Runs all necessary operations to process the configured node against the specified source.
    /// </summary>
    /// <param name="callback">An optional callback that will be invoked to report the state of verification at various stages of the process.</param>
    /// <param name="token">A cancellation token for aborting the operation if required.</param>
    /// <returns>An asynchronous operation representing the completion of the entire process.</returns>
    public async Task RunAll(Action<Verification>? callback = null, CancellationToken token = default)
    {
        Logger.Info($"Starting run for {Source.Name} against {Node.Name}.");
        State = ResultState.Pending;
        _specTotal = Node.Descendants(NodeType.Spec).Count();

        var stopwatch = Stopwatch.StartNew();

        try
        {
            Logger.Info($"Loading source '{Source.Name}'.");
            var target = await Cache.GetOrAdd(source, token);

            Logger.Info($"Running node '{Node.Name}'.");
            State = ResultState.Running;
            var nodeStateChanged = OnNodeStateChanged(callback);
            var verification = await Node.RunAsync(target, nodeStateChanged, token);

            Logger.Info($"'{Source.Name}' returned {verification.State} for '{Node.Name}'.");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"An error occurred while processing '{Source.Name}' against {Node.Name}.");
        }

        stopwatch.Stop();
        Logger.Info($"Run completed in {stopwatch.Elapsed} with result: {Node.Result.State}.");

        State = Node.Result.State;
        RanOn = DateTime.Now;
        RanBy = Environment.UserName;
        Duration = stopwatch.ElapsedMilliseconds;
    }

    /// <summary>
    /// Wraps the given callback to perform additional actions whenever a node's state changes.
    /// </summary>
    /// <param name="original">The original callback to be invoked after processing the node state change.</param>
    /// <returns>A new callback that updates progress and invokes the original callback, if provided.</returns>
    private Action<Verification> OnNodeStateChanged(Action<Verification>? original)
    {
        return verification =>
        {
            EvaluateProgress(verification);
            original?.Invoke(verification);
        };
    }

    /// <summary>
    /// Updates the progress of the execution based on the current verification state.
    /// </summary>
    /// <param name="verification">The verification object containing the result state to evaluate.</param>
    private void EvaluateProgress(Verification verification)
    {
        if (!verification.State.IsDetermined) return;

        _specCount++;

        if (_specTotal > 0)
        {
            Progress = (float)_specCount / _specTotal * 100;
        }
    }
}