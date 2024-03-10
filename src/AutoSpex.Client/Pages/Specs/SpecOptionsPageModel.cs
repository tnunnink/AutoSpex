using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;

namespace AutoSpex.Client.Pages;

public class SpecOptionsPageModel(NodeObserver node) : PageViewModel
{
    public override string Route => $"Node/{node.Id}/Options";
    public override string Title => "Options";
    public override string Icon => "Options";
    public SpecObserver Spec => node.Spec ?? SpecObserver.Empty;
}