using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Pages;

public partial class OutcomesPageModel : PageViewModel, IRecipient<OutcomeObserver.Open>
{
    /// <inheritdoc/>
    public OutcomesPageModel(RunObserver run) : base("Outcomes")
    {
        Run = run;
    }

    public override string Route => $"{nameof(Run)}/{Run.Id}/{Title}";
    public RunObserver Run { get; }

    [ObservableProperty] private OutcomeObserver? _outcome;

    [ObservableProperty] private bool _showResults;

    public void Receive(OutcomeObserver.Open message)
    {
        if (!Run.Outcomes.Any(x => x.Is(message.Outcome))) return;
        Outcome = message.Outcome;
        ShowResults = true;
    }

    protected override void FilterChanged(string? value)
    {
        if (Outcome is null) return;

        var state = Outcome.FilterState;
        var text = value;

        Outcome.Evaluations.Filter(x =>
        {
            var hasState = state == ResultState.None || x.Result == state;
            var hasText = x.Filter(text);
            return hasState && hasText;
        });
    }
}