using System.Threading.Tasks;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Persistence;
using Avalonia.Threading;
using FluentResults;
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
        await base.Load();

        Track(Run);

        //Auto trigger run execution when signaled assuming the run is configured.
        if (runOnLoad && Run.CanExecute())
        {
            await Run.Execute();
        }

        Dispatcher.UIThread.Invoke(() => SaveCommand.NotifyCanExecuteChanged());
    }

    /// <inheritdoc />
    public override async Task<Result> Save()
    {
        var result = await Mediator.Send(new SaveRun(Run));

        if (result.IsFailed)
        {
            NotifySaveFailed(result);
        }
        else
        {
            NotifySaveSuccess();
            AcceptChanges();
        }

        return result;
    }
}