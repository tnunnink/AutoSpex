using System;
using System.Linq;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Observers;

public partial class OutcomeObserver : Observer<Outcome>,
    IRecipient<OutcomeObserver.Running>,
    IRecipient<OutcomeObserver.Complete>
{
    /// <inheritdoc/>
    public OutcomeObserver(Outcome model) : base(model)
    {
        Result = model.Result;
        Evaluations = new ObserverCollection<Evaluation, EvaluationObserver>(
            refresh: () => Model.Evaluations.Select(e => new EvaluationObserver(e)).ToList()
        );
    }

    public override Guid Id => Model.OutcomeId;
    public override string Name => Model.Name;
    public NodeObserver? Node => GetObserver<NodeObserver>(Model.NodeId);
    public long Duration => Model.Duration;

    [ObservableProperty] private ResultState _result = ResultState.None;
    public ObserverCollection<Evaluation, EvaluationObserver> Evaluations { get; }

    /// <inheritdoc />
    public override bool Filter(string? filter)
    {
        //Set text for highlighting from the UI.
        FilterText = filter;

        //Check matches against name and node path.
        return string.IsNullOrEmpty(filter)
                     || Name.Satisfies(filter)
                     || Node?.Model.Path.Satisfies(filter) is true;
    }

    /// <inheritdoc />
    /// <remarks>
    /// To get back to the spec we need to load the node first. If it no longer exists, we can't navigate there.
    /// </remarks>
    protected override async Task Navigate()
    {
        if (Node is null) return;
        await Navigator.Navigate(Node);
    }

    /// <summary>
    /// When we receive the running message for the outcome with the same local id, then we want to set the result
    /// state to pending to notify the UI which outcome is processing.
    /// </summary>
    /// <param name="message"></param>
    public void Receive(Running message)
    {
        if (Id != message.OutcomeId) return;
        Dispatcher.UIThread.Invoke(() => { Result = ResultState.Running; });
    }

    /// <summary>
    /// When we receive the complete message and the outcome is the same as the underlying model, then we want to
    /// update the local state to refresh/notify the Ui the outcome has been processed.
    /// </summary>
    /// <param name="message">The message indicating an outcome run is complete.</param>
    public void Receive(Complete message)
    {
        if (Id != message.Outcome.OutcomeId) return;

        Dispatcher.UIThread.Invoke(() =>
        {
            Result = Model.Result;
            Evaluations.Refresh();
            Refresh();
        });
    }

    /// <summary>
    /// A message send that indicates an outcome is about to be run.
    /// </summary>
    /// <param name="OutcomeId">The ID of the outcome that is about to be run.</param>
    public record Running(Guid OutcomeId);

    /// <summary>
    /// A message sent that indicates an outcome just completed running.
    /// </summary>
    /// <param name="Outcome">The outcome instance that just completed its run.</param>
    public record Complete(Outcome Outcome);

    public static implicit operator Outcome(OutcomeObserver observer) => observer.Model;
    public static implicit operator OutcomeObserver(Outcome model) => new(model);
}