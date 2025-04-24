using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public class RunDetailPageModel(RunObserver run) : PageViewModel
{
    public RunObserver Run { get; } = run;

    protected override void FilterChanged(string? filter)
    {
        var state = Run.FilterState;
        var text = filter;

        Run.Evaluations.Filter(x =>
        {
            var hasState = state == ResultState.None || x.Result == state;
            var hasText = x.Filter(text);
            return hasState && hasText;
        });
    }
}