using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class NodeInfoPageModel(NodeObserver node) : DetailPageModel
{
    public override string Route => $"{Node.Type}/{Node.Id}/{Title}";
    public override string Title => "Info";
    public NodeObserver Node { get; } = node;
}