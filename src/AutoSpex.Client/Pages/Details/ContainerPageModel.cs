using AutoSpex.Client.Observers;
using AutoSpex.Engine;

namespace AutoSpex.Client.Pages;

public class ContainerPageModel(NodeObserver node) : NodePageModel(node)
{
    protected override async Task NavigateTabs()
    {
        await Navigator.Navigate(() => new ContainerNodesPageModel(Node));
        await Navigator.Navigate(() => new NodeVariablesPageModel(Node));
        await Navigator.Navigate(() => new NodeInfoPageModel(Node));
    }

    /// <inheritdoc />
    protected override async Task Run()
    {
        if (Node.Feature == NodeType.Run) return;
        var run = RunObserver.Virtual(Node, out var node);
        await Navigator.Navigate(() => new RunPageModel(node, run));
    }
    
    /// <inheritdoc />
    protected override bool CanRun() => Node.Feature != NodeType.Run;
}