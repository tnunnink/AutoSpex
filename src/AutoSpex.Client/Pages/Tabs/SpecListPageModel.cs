using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public class SpecListPageModel(NodeObserver node) : PageViewModel
{
    public override string Route => $"Node/{Node.Id}/{Title}";
    public override string Title => "Specs";
    public NodeObserver Node { get; } = node;
}