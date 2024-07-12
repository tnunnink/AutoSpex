using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public class RunDetailPageModel(RunObserver run) : DetailPageModel
{
    public override string Route => $"{nameof(Run)}/{Run.Id}";
    public override string Title => Run.Name;
    public override string Icon => nameof(Run);
    public RunObserver Run { get; } = run;

    /// <inheritdoc />
    /// <remarks>
    /// For a run we expect the loaded run observer instance, and upon loading we should execute the run.
    /// </remarks>
    public override async Task Load()
    {
        await Run.RunCommand.ExecuteAsync(null);
    }
}