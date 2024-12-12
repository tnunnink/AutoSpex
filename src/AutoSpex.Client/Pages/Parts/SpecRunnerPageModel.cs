using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoSpex.Client.Pages;

public partial class SpecRunnerPageModel(NodeObserver node) : PageViewModel("Runner")
{
    public override string Route => $"Spec/Runner/{node.Id}";

    [ObservableProperty] private OutcomeObserver? _outcome;

    /// <summary>
    /// Command to run the spec locally against the target source. This sets the resulting "virtual" outcome instance
    /// which we can use to show the result data in our results drawer. This is to let the user test a spec without
    /// having to save and then create a run instance for a single item.
    /// </summary>
    public async Task Run(Spec? spec)
    {
        if (spec is null) return;
        Outcome = null;

        var result = await Mediator.Send(new LoadTargetSource());
        if (Notifier.ShowIfFailed(result)) return;

        var content = result.Value.Content;
        if (content is null)
        {
            Notifier.ShowError($"Failed to run specification {node.Name}",
                "No source is targeted. Add and/or select a target source in order to run this specification.");
            return;
        }

        var verification = await spec.RunAsync(content);
        Outcome = new Outcome { NodeId = node.Id, Name = node.Name, Verification = verification };
    }

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