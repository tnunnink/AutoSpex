using System;
using System.Collections.Generic;
using System.Linq;
using AutoSpex.Client.Resources;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Observers;

public partial class OutcomeObserver : Observer<Outcome>,
    IRecipient<OutcomeObserver.Running>,
    IRecipient<OutcomeObserver.Complete>
{
    /// <inheritdoc/>
    public OutcomeObserver(Outcome model) : base(model)
    {
        Result = model.Verification.Result;
        Evaluations = new ObserverCollection<Evaluation, EvaluationObserver>(
            refresh: () => Model.Verification.Evaluations.Select(e => new EvaluationObserver(e)).ToList(),
            count: () => Model.Verification.Evaluations.Count()
        );
        RegisterDisposable(Evaluations);
    }

    protected override bool PromptForDeletion => false;
    public override Guid Id => Model.OutcomeId;
    public override string Name => Model.Name;
    public NodeObserver? Node => GetObserver<NodeObserver>(x => x.Id == Model.NodeId);

    [ObservableProperty] private ResultState _result = ResultState.None;
    public long Duration => Model.Verification.Duration;
    public int Total => Model.Verification.Evaluations.Count();
    public int Passed => Model.Verification.Evaluations.Count(e => e.Result == ResultState.Passed);
    public int Failed => Model.Verification.Evaluations.Count(e => e.Result == ResultState.Failed);
    public int Errored => Model.Verification.Evaluations.Count(e => e.Result == ResultState.Errored);
    public ObserverCollection<Evaluation, EvaluationObserver> Evaluations { get; }

    [ObservableProperty] private ResultState _filterState = ResultState.None;

    /// <inheritdoc />
    public override bool Filter(string? filter)
    {
        //Set text for highlighting from the UI.
        FilterText = filter;

        //Check matches against name and node path.
        return string.IsNullOrEmpty(filter)
               || Name.Satisfies(filter)
               || Node?.Filter(filter) is true;
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
    /// Applies a filter to the evaluations in the OutcomeObserver based on the specified ResultState.
    /// </summary>
    /// <param name="state">The ResultState to filter by.</param>
    [RelayCommand]
    private void ApplyFilter(ResultState? state)
    {
        FilterState = state ?? ResultState.None;
        Evaluations.Filter(x => FilterState == ResultState.None || x.Result == FilterState);
    }

    /// <summary>
    /// When we receive the running message for the outcome with the same local id, then we want to set the result
    /// state to pending to notify the UI which outcome is processing.
    /// </summary>
    /// <param name="message"></param>
    public void Receive(Running message)
    {
        if (Model.RunId != message.Outcome.RunId || Model.NodeId != message.Outcome.NodeId) return;

        Dispatcher.UIThread.Invoke(() => { Result = ResultState.Running; });
    }

    /// <summary>
    /// When we receive the complete message and the outcome is the same as the underlying model, then we want to
    /// update the local state to refresh/notify the Ui the outcome has been processed.
    /// </summary>
    /// <param name="message">The message indicating an outcome run is complete.</param>
    public void Receive(Complete message)
    {
        if (Model.RunId != message.Outcome.RunId || Model.NodeId != message.Outcome.NodeId) return;

        Dispatcher.UIThread.Invoke(() =>
        {
            Result = message.Outcome.Verification.Result;
            Evaluations.Refresh();
            Refresh();
        });
    }

    /// <summary>
    /// A message send that indicates an outcome is about to be run.
    /// </summary>
    /// <param name="Outcome">The outcome that is about to run.</param>
    public record Running(Outcome Outcome);

    /// <summary>
    /// A message sent that indicates an outcome just completed running.
    /// </summary>
    /// <param name="Outcome">The outcome produced by the run.</param>
    public record Complete(Outcome Outcome);

    /// <inheritdoc />
    protected override IEnumerable<MenuActionItem> GenerateContextItems()
    {
        yield return new MenuActionItem
        {
            Header = "Open Spec",
            Icon = Resource.Find("IconLineLaunch"),
            Command = NavigateCommand,
            Gesture = new KeyGesture(Key.Enter)
        };

        yield return new MenuActionItem
        {
            Header = "Suppress Result",
            Icon = Resource.Find("IconFilledTrash"),
        };
    }

    public static implicit operator Outcome(OutcomeObserver observer) => observer.Model;
    public static implicit operator OutcomeObserver(Outcome model) => new(model);
}