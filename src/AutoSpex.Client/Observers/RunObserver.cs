﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentResults;

namespace AutoSpex.Client.Observers;

public partial class RunObserver : Observer<Run>
{
    private CancellationTokenSource? _cancellation;

    /// <inheritdoc/>
    public RunObserver(Run model) : base(model)
    {
        Result = Model.Result;
        Outcomes = new ObserverCollection<Outcome, OutcomeObserver>(
            refresh: () => Model.Outcomes.Select(x => new OutcomeObserver(x)).ToList(),
            count: () => Model.Outcomes.Count()
        );

        Track(nameof(Result));
        Track(Outcomes);
    }

    public override Guid Id => Model.RunId;
    public override string Name => Model.Name;
    public override string Icon => nameof(Run);
    public DateTime RanOn => Model.RanOn;
    public string RanBy => Model.RanBy;
    public int Total => Model.Outcomes.Count();
    public int Ran => Model.Outcomes.Count(x => x.Verification.Result > ResultState.Pending);
    public int Passed => Model.Outcomes.Count(x => x.Verification.Result == ResultState.Passed);
    public int Failed => Model.Outcomes.Count(x => x.Verification.Result == ResultState.Failed);
    public int Errored => Model.Outcomes.Count(x => x.Verification.Result == ResultState.Errored);
    public int Inconclusive => Model.Outcomes.Count(x => x.Verification.Result == ResultState.Inconclusive);
    public long Duration => Model.Outcomes.Sum(x => x.Verification.Duration);
    public bool HasResult => Model.Result > ResultState.Pending;
    public float Progress => Total > 0 ? (float)Ran / Total * 100 : 0;
    public NodeObserver Node => new(Model.Node);
    public SourceObserver Source => new(Model.Source);
    public ObserverCollection<Outcome, OutcomeObserver> Outcomes { get; }

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(CancelCommand))]
    private ResultState _result;

    [ObservableProperty] private ResultState _filterState = ResultState.None;

    /// <summary>
    /// Command to execute this run by retrieving, resolving, and evaluating all configured spec/source pairs and
    /// producing new outcome results.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExecute))]
    public async Task Execute()
    {
        _cancellation = new CancellationTokenSource();
        var token = _cancellation.Token;

        MarkPending();

        await Task.Delay(1000, token);

        try
        {
            var loadSource = await LoadSource(token);
            if (Notifier.ShowIfFailed(loadSource)) return;
            var source = loadSource.Value;

            var nodes = (await LoadSpecs(token)).ToList();

            await Model.Execute(nodes, source, OnSpecRunning, OnSpecCompleted, token);
        }
        catch (OperationCanceledException)
        {
            Notifier.ShowWarning("Run canceled", $"{Name} was canceled prior to finishing execution.");
        }

        Result = Model.Result;
        OnPropertyChanged(string.Empty);
    }

    /// <summary>
    /// Indicates that this run can be executed.
    /// </summary>
    public bool CanExecute()
    {
        return Model.Node.NodeId != Guid.Empty && Model.Source.SourceId != Guid.Empty;
    }

    /// <summary>
    /// Command to cancel execution of this run.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanCancel))]
    private void Cancel() => _cancellation?.Cancel();

    /// <summary>
    /// Indicates that the run can be cancelled.
    /// </summary>
    private bool CanCancel() => Result == ResultState.Pending;

    /// <summary>
    /// Applies a filter to the outcomes of this run based on the provided result state value.
    /// </summary>
    /// <param name="state">The <see cref="ResultState"/> to filter by.</param>
    [RelayCommand]
    private void ApplyFilter(ResultState? state)
    {
        FilterState = state ?? ResultState.None;
    }

    /// <summary>
    /// When the filter text for a run observer changes
    /// </summary>
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName is nameof(FilterText) or nameof(FilterState))
            ApplyFilters(FilterState, FilterText);
    }

    /// <summary>
    /// Loads the full source object configured for this run. This is the source that all specs will be run against.
    /// </summary>
    private async Task<Result<Source>> LoadSource(CancellationToken token)
    {
        var sourceId = Model.Source.SourceId;
        var result = await Mediator.Send(new LoadSource(sourceId), token);
        return result;
    }

    /// <summary>
    /// Load all specs configured for this run. This is executed prior to each run to get the most updated
    /// configuration to execute.
    /// </summary>
    private async Task<IEnumerable<Node>> LoadSpecs(CancellationToken token)
    {
        var ids = Model.Outcomes.Select(n => n.NodeId);
        var result = await Mediator.Send(new LoadNodes(ids), token);
        return result.IsSuccess ? result.Value : [];
    }

    /// <summary>
    /// Sends a message that the provided outcome has started running so that the observer can update it's visual state.
    /// </summary>
    /// <param name="outcome">The outcome that is starting to run.</param>
    private void OnSpecRunning(Outcome outcome)
    {
        var message = new OutcomeObserver.Running(outcome);
        Messenger.Send(message);
    }

    /// <summary>
    /// Sends a message that the provided outcome has completed running and its state is updated with the result.
    /// </summary>
    /// <param name="outcome">The outcome that just ran.</param>
    private void OnSpecCompleted(Outcome outcome)
    {
        var message = new OutcomeObserver.Complete(outcome);
        Messenger.Send(message);
        
        OnPropertyChanged(nameof(Ran));
        OnPropertyChanged(nameof(Progress));
    }

    /// <summary>
    /// Sets this run and all outcomes configured to a pending result state to indicate to the UI that these specs
    /// are awaiting a new result. 
    /// </summary>
    private void MarkPending()
    {
        Result = ResultState.Pending;

        foreach (var outcome in Outcomes)
        {
            outcome.Result = ResultState.Pending;
        }
    }

    /// <summary>
    /// Applies the configured result state filter and filter text to the observer collection.
    /// </summary>
    private void ApplyFilters(ResultState state, string? text)
    {
        Outcomes.Filter(x =>
        {
            var hasState = state == ResultState.None || x.Result == state;
            var hasText = x.Filter(text);
            return hasState && hasText;
        });
    }

    public static implicit operator Run(RunObserver observer) => observer.Model;
    public static implicit operator RunObserver(Run model) => new(model);
}