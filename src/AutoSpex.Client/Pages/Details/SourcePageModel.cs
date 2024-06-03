using AutoSpex.Client.Observers;
using CommunityToolkit.Mvvm.ComponentModel;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public class SourcePageModel(NodeObserver node) : NodePageModel(node)
{
    protected override async Task NavigateTabs()
    {
        await Navigator.Navigate(() => new SourceContentPageModel(Node));
        await Navigator.Navigate(() => new NodeVariablesPageModel(Node));
        await Navigator.Navigate(() => new NodeInfoPageModel(Node));
    }
}