using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.ComponentModel;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class ResultPageModel : PageViewModel
{
    [ObservableProperty] private ResultObserver? _result;

    protected override void FilterChanged(string? filter)
    {
        if (Result is null) return;

        var state = Result.FilterState;
        var text = filter;

        Result.Evaluations.Filter(x =>
        {
            var hasState = state == ResultState.None || x.Result == state;
            var hasText = x.Filter(text);
            return hasState && hasText;
        });
    }
}