using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public class SpecSettingsPageModel(SpecObserver spec) : PageViewModel
{
    public override string Route => $"Node/{Spec.Id}/{Title}";
    public override string Title => "Settings";
    public SpecObserver Spec { get; } = spec;
}