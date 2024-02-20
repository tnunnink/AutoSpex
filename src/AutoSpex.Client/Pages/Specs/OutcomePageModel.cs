using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoSpex.Client.Pages;

public partial class OutcomePageModel(SpecObserver spec) : PageViewModel
{
    public SpecObserver Spec { get; } = spec;

    [ObservableProperty] private Outcome? _outcome;
}