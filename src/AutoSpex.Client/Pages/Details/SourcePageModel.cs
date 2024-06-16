using AutoSpex.Client.Observers;
using AutoSpex.Engine;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public class SourcePageModel(NodeObserver node) : NodePageModel(node)
{
    /// <inheritdoc />
    protected override async Task NavigateTabs()
    {
        await Navigator.Navigate(() => new SourceContentPageModel(Node));
        await Navigator.Navigate(() => new NodeVariablesPageModel(Node));
        await Navigator.Navigate(() => new NodeInfoPageModel(Node));
    }
}