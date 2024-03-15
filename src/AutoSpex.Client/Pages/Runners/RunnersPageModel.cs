using System.Collections.ObjectModel;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.Input;

namespace AutoSpex.Client.Pages;

public partial class RunnersPageModel : PageViewModel
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
        var runner = RunnerObserver.New;
        
        var result = await Mediator.Send(new CreateRunner(runner.Model));
        if (result.IsFailed) return;
        
        Runners.Add(runner);
        await Navigator.Navigate(runner);
    }
}