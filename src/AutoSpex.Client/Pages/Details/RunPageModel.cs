using AutoSpex.Client.Observers;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class RunPageModel(NodeObserver node) : NodePageModel(node)
{
    [ObservableProperty] private RunObserver? _run;

    [ObservableProperty] private SourceObserver? _source;

    private async Task LoadSource()
    {
        var result = await Mediator.Send(new GetSource(Node.Id));
        if (result.IsFailed) return;
        Source = result.Value;
    }
}