using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;

namespace AutoSpex.Client.Pages;

public class CollectionDetailPageModel(NodeObserver node) : PageViewModel
{
    public override string Route => $"{Node.Type}/{Node.Id}/Config";
    public NodeObserver Node { get; } = node;
}