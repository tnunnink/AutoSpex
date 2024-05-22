using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public class NodeInfoPageModel(NodeObserver node) : PageViewModel
{
    public override string Route => $"{Node.Type}/{Node.Id}/{Title}";
    public override string Title => "Info";
    public NodeObserver Node { get; } = node;
}