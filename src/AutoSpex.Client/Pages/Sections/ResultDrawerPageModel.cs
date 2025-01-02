using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoSpex.Client.Pages;

public partial class ResultDrawerPageModel : PageViewModel
{
    [ObservableProperty] private OutcomeObserver? _outcome;

    protected override void FilterChanged(string? filter)
    {
        if (Outcome is null) return;

        var state = Outcome.FilterState;
        var text = filter;

        Outcome.Evaluations.Filter(x =>
        {
            var hasState = state == ResultState.None || x.Result == state;
            var hasText = x.Filter(text);
            return hasState && hasText;
        });
    }
}