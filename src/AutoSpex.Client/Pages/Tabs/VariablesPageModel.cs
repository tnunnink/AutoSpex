using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;

namespace AutoSpex.Client.Pages;

public class VariablesPageModel(NodeObserver node) : PageViewModel("Variables")
{
    public override string Route => $"{node.Type}/{node.Id}/{Title}";
    public override string Icon => "IconLineAt";
}