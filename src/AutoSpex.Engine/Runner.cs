using System.Collections.Concurrent;

namespace AutoSpex.Engine;

public static class Runner
{
    /// <summary>
    /// Executes the provided run instance and returns the result state.
    /// </summary>
    /// <param name="run">The run instance to be executed.</param>
    /// <param name="callback">An optional callback invoked when a run changes state.</param>
    /// <param name="token">An optional cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation and contains the resulting state of the run.</returns>
    public static Task<RunResult> Run(Run run, Action<Run>? callback = null, CancellationToken token = default)
    {
        return run.Execute(callback, token);
    }

    /// <summary>
    /// Executes an array of runs in parallel and returns the aggregated result state.
    /// </summary>
    /// <param name="runs">The array of runs to be executed.</param>
    /// <param name="callback">An optional callback invoked when a run changes state.</param>
    /// <param name="token">An optional cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation and contains the final aggregated result state of the runs.</returns>
    public static async Task<IEnumerable<RunResult>> Run(Run[] runs,
        Action<Run>? callback = null,
        CancellationToken token = default)
    {
        var options = new ParallelOptions
        {
            //MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount / 4),
            MaxDegreeOfParallelism = Environment.ProcessorCount,
            CancellationToken = token
        };

        var results = new ConcurrentBag<RunResult>();

        await Parallel.ForEachAsync(runs, options, async (run, cancel) =>
        {
            var result = await run.Execute(callback, cancel);
            results.Add(result);
        });

        return results;
    }
}