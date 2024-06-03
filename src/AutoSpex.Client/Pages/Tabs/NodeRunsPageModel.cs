using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public class NodeRunsPageModel(NodeObserver node) : DetailPageModel
{
    public override string Route => $"{node.Type}/{node.Id}/{Title}";
    public override string Title => "Runs";
}