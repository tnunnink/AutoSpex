using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;

namespace AutoSpex.Client.Pages;

public class FiltersPageModel(NodeObserver node) : PageViewModel
{
    public override string Route => $"Node/{node.Id}/Filters";
    public override string Title => "Filters";
    public override string Icon => "Filters";
    public SpecObserver Spec => node.Spec ?? SpecObserver.Empty;
}