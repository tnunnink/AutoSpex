using System;
using System.Collections.ObjectModel;
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
        Evaluations = new ObservableCollection<EvaluationObserver>(
            Model.Evaluations.Select(e => new EvaluationObserver(e)));
    }

    public override Guid Id => Model.OutcomeId;
    public Guid SpecId => Model.Spec?.NodeId ?? Guid.Empty;
    public Guid SourceId => Model.Source?.NodeId ?? Guid.Empty;
    public string SpecName => Model.Spec?.Name ?? string.Empty;
    public string SpecPath => Model.Spec?.Path ?? string.Empty;
    public string SourceName => Model.Source?.Name ?? string.Empty;
    public long Duration => Model.Duration;

    [ObservableProperty] private ResultState _result;
    public ObservableCollection<EvaluationObserver> Evaluations { get; }

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
        if (!Model.Equals(message.Outcome)) return;

        Dispatcher.UIThread.Invoke(() =>
        {
            Result = Model.Result;
            Refresh();
            Evaluations.Refresh(Model.Evaluations.Select(e => new EvaluationObserver(e)));
        });
    }

    public override bool Filter(string? filter)
    {
        return base.Filter(filter)
               || SpecName.PassesFilter(filter)
               || SourceName.PassesFilter(filter)
               || SpecPath.PassesFilter(filter)
               || Result.ToString().PassesFilter(filter)
               || Evaluations.Any(e => e.Filter(filter));
    }

    public static implicit operator Outcome(OutcomeObserver observer) => observer.Model;
    public static implicit operator OutcomeObserver(Outcome model) => new(model);

    /// <summary>
    /// A message send that indicates an outcome is about to be run.
    /// </summary>
    /// <param name="OutcomeId">The ID of the outcome that is about to be run.</param>
    public record Running(Guid OutcomeId);

    /// <summary>
    /// A message sent that indicates an outcome just completed running..
    /// </summary>
    /// <param name="Outcome">The outcome instance that just completed its run.</param>
    public record Complete(Outcome Outcome);
}