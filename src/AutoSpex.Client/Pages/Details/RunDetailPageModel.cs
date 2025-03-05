using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.Input;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class RunDetailPageModel(RunObserver run) : DetailPageModel(run.Name)
{
    public override string Route => $"{nameof(Run)}/{Run.Id}";
    public override string Icon => nameof(Run);
    public RunObserver Run { get; private set; } = run;

    /// <inheritdoc />
    /// <remarks>
    /// This page expects the provided run object to be fully loaded. 
    /// Only execute runs that have not been run (i.e. new run instances).
    /// </remarks>
    public override async Task Load()
    {
        CurrentPage = await Navigator.Navigate(() => new OutcomesPageModel(Run));

        if (Run.Result != ResultState.None) return;
        await Run.Execute();

        Notifier.ShowSuccess("Run completed successfully",
            $"{Run.Node.Name} {Run.Result} for {Run.Source.Name} in {Run.Duration}.");
    }

    [RelayCommand]
    private async Task RunAgain()
    {
        //Create a new run instance.
        var result = await Mediator.Send(new NewRun(Run.Node.Id, Run.Source.Id));
        if (Notifier.ShowIfFailed(result)) return;

        //Replace the current run object.
        Run = new RunObserver(result.Value);
        OnPropertyChanged(nameof(Run));

        //Reload this page, which will execute the new run instance.
        await Load();
    }
}