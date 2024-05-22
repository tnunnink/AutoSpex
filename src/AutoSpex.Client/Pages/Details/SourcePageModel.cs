using AutoSpex.Client.Observers;
using CommunityToolkit.Mvvm.ComponentModel;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class SourcePageModel(NodeObserver node) : NodePageModel(node)
{
    public override async Task Load()
    {
        await NavigateTabs();
    }

    private async Task NavigateTabs()
    {
        await Navigator.Navigate(() => new SourceContentPageModel(Node));
        await Navigator.Navigate(() => new NodeInfoPageModel(Node));
    }
}