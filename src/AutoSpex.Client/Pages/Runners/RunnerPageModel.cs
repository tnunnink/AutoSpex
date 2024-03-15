using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoSpex.Client.Pages;

public partial class RunnerPageModel(RunnerObserver runner) : DetailPageModel
{
    public override string Route => $"Runner/{runner.Id}";
    public override string Title => runner.Name;
    public override string Icon => "Runner";
    
    [ObservableProperty] private RunnerObserver? _runner;

    public override async Task Load()
    {
        var result = await Mediator.Send(new GetRunner(runner.Id));
        if (result.IsFailed) return;
        Runner = new RunnerObserver(result.Value);
    }
}