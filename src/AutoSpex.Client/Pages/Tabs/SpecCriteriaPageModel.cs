using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public class SpecCriteriaPageModel(SpecObserver spec) : PageViewModel
{
    public override string Route => $"Node/{Spec.Id}/{Title}";
    public override string Title => "Criteria";
    public SpecObserver Spec { get; } = spec;
}