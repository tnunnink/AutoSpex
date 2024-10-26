using System.Threading.Tasks;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public class RunDetailPageModel(RunObserver run, bool runOnLoad = false) : DetailPageModel(run.Name)
{
    public override string Route => $"{nameof(Run)}/{Run.Id}";
    public override string Icon => nameof(Run);
    public RunObserver Run { get; } = run;
    public Task<OutcomesPageModel> OutcomePage => Navigator.Navigate(() => new OutcomesPageModel(Run));

    /// <inheritdoc />
    public override async Task Load()
    {
        //Auto trigger run execution when signaled assuming the run is configured.
        if (runOnLoad && Run.CanExecute())
        {
            await Run.Execute();
        }
    }
}