using System.Diagnostics;
using NLog;

namespace AutoSpex.Engine;

public static class Runner
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static readonly SourceCache Cache = SourceCache.Default;

    public static async Task<RunResult> Run(RunContext config, CancellationToken token = default)
    {
        if (config == null) throw new ArgumentNullException(nameof(config));
        if (config.Node == null)
            throw new ArgumentException("The run configuration must specify a node to run.", nameof(config));

        Logger.Info($"Starting run for configuration '{config.Name}' against {config.Sources.Count} sources.");
        var stopwatch = Stopwatch.StartNew();

        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount,
            CancellationToken = token
        };

        await Parallel.ForEachAsync(config.Sources, options, async (source, cancel) =>
        {
            try
            {
                Logger.Info($"Loading source '{source.Name}'.");
                var cached = await Cache.GetOrAdd(source, cancel);
                Logger.Info($"Running source '{source.Name}' against node '{config.Node.Name}'.");
                var verification = await config.Node.RunAsync(cached, token: cancel);
                Logger.Info($"Source '{source.Name}' completed with result: {verification.Result}.");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"An error occurred while processing source '{source.Name}'.");
            }
        });

        stopwatch.Stop();

        var run = new RunResult(config.Node);
        Logger.Info($"Run completed in {stopwatch.Elapsed} with result: {run.Result}.");
        return run;
    }
}