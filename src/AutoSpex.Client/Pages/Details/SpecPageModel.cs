using AutoSpex.Client.Observers;

namespace AutoSpex.Client.Pages;

public class SpecPageModel(NodeObserver node) : NodePageModel(node)
{
    /// <inheritdoc />
    protected override async Task NavigateTabs()
    {
        await Navigator.Navigate(() => new SpecCriteriaPageModel(Node));
        await Navigator.Navigate(() => new NodeVariablesPageModel(Node));
        await Navigator.Navigate(() => new NodeInfoPageModel(Node));
    }
}