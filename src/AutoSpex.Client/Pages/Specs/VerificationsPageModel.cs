using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;

namespace AutoSpex.Client.Pages;

public class VerificationsPageModel(NodeObserver node) : PageViewModel
{
    public override string Route => $"Node/{node.Id}/Verifications";
    public override string Title => "Verifications";
    public override string Icon => "Verifications";
    public SpecObserver Spec => node.Spec ?? SpecObserver.Empty;
}