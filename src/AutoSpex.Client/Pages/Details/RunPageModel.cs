using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class RunPageModel(RunObserver run) : DetailPageModel
{
    public override string Route => $"Run/{Run.Id}";
    public override string Title => Run.Name;
    public override string Icon => "Run";

    [ObservableProperty] private RunObserver _run = run;
    
    [ObservableProperty] private SourceObserver? _source;

    public override async Task Load()
    {
        await LoadSource();

        if (Run.RunOnLoad)
        {
            await Run.Execute();
        }
    }
    
    private async Task LoadSource()
    {
        var result = await Mediator.Send(new GetSource(Run.Model.SourceId));
        if (result.IsFailed) return;
        Source = result.Value;
    }
}