using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AutoSpex.Client.Pages;

public partial class SpecRunnerPageModel(NodeObserver node) : PageViewModel("Runner")
{
    public override string Route => $"/{node.Id}/{Title}";

    [ObservableProperty] private OutcomeObserver? _outcome;

    /// <summary>
    /// Command to run the spec locally against the target source. This sets the resulting "virtual" outcome instance
    /// which we can use to show the result data in our results drawer. This is to let the user test a spec without
    /// having to save and then create a run instance for a single item.
    /// </summary>
    [RelayCommand]
    private async Task Test()
    {
        Outcome = null;

        var result = await Mediator.Send(new LoadTargetSource());
        if (Notifier.ShowIfFailed(result)) return;

        var content = result.Value.Content;
        var verification = await node.Model.Run(content); //todo could add cancellation here

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