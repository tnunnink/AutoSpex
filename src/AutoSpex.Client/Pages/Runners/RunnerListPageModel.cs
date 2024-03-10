using System.Collections.ObjectModel;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.Input;

namespace AutoSpex.Client.Pages;

public partial class RunnerListPageModel : PageViewModel
{
    public override string Title => "Runners";
    public override string Icon => "Runners";
    public ObservableCollection<RunnerObserver> Runners { get; } = [];


    public override async Task Load()
    {
        var result = await Mediator.Send(new ListRunners());
        if (result.IsFailed) return;

        Runners.Clear();
        foreach (var runner in result.Value)
        {
            Runners.Add(new RunnerObserver(runner));
        }
    }

    [RelayCommand]
    private async Task AddRunner()
    {
        var runner = new Runner();
        var result = await Mediator.Send(new CreateRunner(runner));
        if (result.IsFailed) return;
        Runners.Add(new RunnerObserver(runner));
    }
}