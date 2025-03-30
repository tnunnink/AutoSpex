using NLog;

namespace AutoSpex.Engine;

public static class Runner
{
    /// <summary>
    /// Executes the provided run instance and returns the result state.
    /// </summary>
    /// <param name="run">The run instance to be executed.</param>
    /// <param name="callback">An optional callback invoked when a node changes state.</param>
    /// <param name="token">An optional cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation and contains the resulting state of the run.</returns>
    public static async Task<ResultState> Run(Run run, Action<Verification>? callback = null,
        CancellationToken token = default)
    {
        await run.RunAll(callback, token);
        return run.State;
    }

    /// <summary>
    /// Executes an array of runs in parallel and returns the aggregated result state.
    /// </summary>
    /// <param name="runs">The array of runs to be executed.</param>
    /// <param name="callback">An optional callback invoked when a verification changes state.</param>
    /// <param name="token">An optional cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation and contains the final aggregated result state of the runs.</returns>
    public static async Task<ResultState> Run(Run[] runs, Action<Verification>? callback = null,
        CancellationToken token = default)
    {
        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount,
            CancellationToken = token
        };

        await Parallel.ForEachAsync(runs, options, async (run, cancel) => { await run.RunAll(callback, cancel); });

        return ResultState.MaxOrDefault(runs.Select(r => r.State).ToArray());
    }
}