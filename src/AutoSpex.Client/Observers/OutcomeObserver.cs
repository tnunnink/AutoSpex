using System;
using System.Collections.Generic;
using System.Linq;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Observers;

public partial class OutcomeObserver : Observer<Outcome>,
    IRecipient<OutcomeObserver.Running>,
    IRecipient<OutcomeObserver.Complete>
{
    /// <inheritdoc/>
    public OutcomeObserver(Outcome model, ResultState? filter = default) : base(model)
    {
        Result = model.Result;
        ResultFilter = filter ?? ResultState.None;
        Results = new ObserverCollection<Evaluation, EvaluationObserver>(GetResults);
    }

    public override Guid Id => Model.SpecId;
    public override string Name => Model.Name;
    public NodeObserver? Node => FindInstance<NodeObserver>();
    public long Duration => Model.Duration;

    [ObservableProperty] private ResultState _result = ResultState.None;
    public ObserverCollection<Evaluation, EvaluationObserver> Results { get; }
    public int Passed => Results.Count(x => x.Result == ResultState.Passed);
    public int Failed => Results.Count(x => x.Result == ResultState.Failed);
    public int Errored => Results.Count(x => x.Result == ResultState.Error);

    [ObservableProperty] private ResultState _resultFilter = ResultState.None;

    [ObservableProperty] private IEnumerable<SourceObserver> _selectedSources = [];

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
        if (!Model.Equals(message.Outcome)) return;

        Dispatcher.UIThread.Invoke(() =>
        {
            Result = Model.Result;
            Results.Refresh();
            Refresh();
        });
    }

    /// <inheritdoc />
    public override bool Filter(string? filter)
    {
        FilterText = filter;

        var passes = string.IsNullOrEmpty(filter)
                     || Name.Satisfies(filter)
                     || Node?.Model.Path.Satisfies(filter) is true;
        
        //Updates nested result with the current text filter.
        Results.Refresh();
        
        //Expand only if there are results that pass the filter (and there is an active filter.
        IsExpanded = !string.IsNullOrEmpty(filter) && Results.Count > 0;
        
        return passes || Results.Count > 0;
    }

    /// <summary>
    /// When the <see cref="SelectedSources"/> changes we want to refresh the Results collection to only show the
    /// requested source evaluations.
    /// </summary>
    // ReSharper disable once UnusedParameterInPartialMethod
    partial void OnSelectedSourcesChanged(IEnumerable<SourceObserver> value)
    {
        Results.Refresh();
    }

    /// <summary>
    /// Gets the collection of <see cref="EvaluationObserver"/> to display for this outcome. This method will use the
    /// configured <see cref="ResultFilter"/> and <see cref="SelectedSources"/> to filter the collection.
    /// We are also flattening all results from each verification into a single list.
    /// </summary>
    private List<EvaluationObserver> GetResults()
    {
        var filter = FilterText;
        var state = ResultFilter;
        var sources = SelectedSources.Select(x => x.Id).ToHashSet();

        var results = Model.Verifications
            .SelectMany(v => v.Evaluations)
            .Where(e => state == ResultState.None || e.Result == state)
            .Where(e => sources.Count == 0 || sources.Contains(e.SourceId))
            .Select(e => new EvaluationObserver(e))
            .Where(e => e.Filter(filter));

        return results.ToList();
    }

    /// <summary>
    /// Notifies that the node for this outcome no longer exists and can not be navigated to.
    /// </summary>
    private void NotifyNodeNotFound()
    {
        const string title = "Node not found";
        var message = $"{Name} no longer exists in any collection.";
        Notifier.NotifyError(title, message);
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