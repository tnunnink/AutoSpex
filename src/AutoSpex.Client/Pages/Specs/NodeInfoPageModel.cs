using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;

namespace AutoSpex.Client.Pages;

public class NodeInfoPageModel(NodeObserver node) : PageViewModel
{
    public override string Route => $"Node/{node.Id}/Info";
    public override string Title => "Info";
    public override string Icon => "Info";
    public NodeObserver Node => node;
}