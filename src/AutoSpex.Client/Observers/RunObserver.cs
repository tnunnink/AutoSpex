using System;
using System.Collections.Generic;
using System.Linq;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Observers;

public partial class RunObserver : Observer<Run>, IRecipient<RunObserver.StateChange>
{
    /// <inheritdoc/>
    public RunObserver(Run model) : base(model)
    {
        Verifications = new ObserverCollection<Verification, VerificationObserver>(
            refresh: () => Model.Results.Select(x => new VerificationObserver(x)).ToList(),
            count: () => Model.Results.Count
        );

        Evaluations = new ObserverCollection<Evaluation, EvaluationObserver>(
            refresh: () => Model.Results.SelectMany(x => x.Evaluations).Select(e => new EvaluationObserver(e)).ToList(),
            count: () => Model.Results.SelectMany(e => e.Evaluations).Count()
        );

        Runs = new ObserverCollection<Run, RunObserver>(
            refresh: () => Model.Runs.Select(x => new RunObserver(x)).ToList(),
            count: () => Model.Runs.Count
        );

        RegisterDisposable(Verifications);
        RegisterDisposable(Evaluations);
        RegisterDisposable(Runs);
    }

    public override Guid Id => Model.RunId;
    public override string Name => Model.Node.Name;
    public string SourceName => $"({Model.Source.Name})";
    public ResultState Result => Model.Result;
    public string Duration => $"{Model.Duration} ms";
    public ObserverCollection<Verification, VerificationObserver> Verifications { get; }
    public ObserverCollection<Evaluation, EvaluationObserver> Evaluations { get; }
    public ObserverCollection<Run, RunObserver> Runs { get; }
    public string RunCount => $"({Runs.Count})";

    public int FilterCount { get; set; }
    public int PassedCount => Verifications.Count(e => e.Result == ResultState.Passed);
    public int FailedCount => Verifications.Count(e => e.Result == ResultState.Failed);
    public int ErroredCount => Verifications.Count(e => e.Result == ResultState.Errored);
    public IEnumerable<ResultState> States => GetDistinctResultStates();

    [ObservableProperty] private ResultState _filterState = ResultState.None;

    [ObservableProperty] private string _selectedView = "Results";

    /// <summary>
    /// Filters the tree structure based on the provided filter string recursively.
    /// </summary>
    /// <param name="filter">The filter string to apply.</param>
    /// <param name="state">The result state </param>
    /// <returns>True if any node in the tree structure is visible after filtering; otherwise, false.</returns>
    public bool FilterTree(string? filter, ResultState state)
    {
        var hasText = base.Filter(filter) || SourceName.Satisfies(filter) || RunCount.Satisfies(filter);
        var hasState = state == ResultState.None || state == Result;
        var children = Runs.Count(x => x.FilterTree(filter, state));

        IsVisible = (hasText && hasState) || children > 0;
        IsExpanded = string.IsNullOrEmpty(filter) ? IsExpanded : children > 0;

        return IsVisible;
    }

    /// <summary>
    /// Filters the <see cref="Verifications"/> for this run instance using the provided filer text and configured
    /// <see cref="FilterState"/>.
    /// </summary>
    /// <param name="filter">The filter text to apply.</param>
    protected void FilterEvaluations(string? filter)
    {
        Evaluations.Filter(x =>
        {
            var hasState = FilterState == ResultState.None || x.Result == FilterState;
            var hasText = x.Filter(filter);
            return hasState && hasText;
        });
    }

    /// <summary>
    /// 
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanRun))]
    private void Run()
    {
        Messenger.Send(new ExecuteRun(this));
    }

    /// <summary>
    /// 
    /// </summary>
    private bool CanRun() => Result.IsProcessing;

    /// <summary>
    /// Sets the <see cref="FilterState"/> of this result object which will in turn filter the <see cref="Verifications"/>
    /// based on the provided state.
    /// </summary>
    [RelayCommand]
    private void ApplyFilterState(ResultState state)
    {
        FilterState = state;
        Refresh();
    }

    /// <summary>
    /// Command to set the local <see cref="SelectedView"/> proeprty to toggle between results, data, and logs for
    /// the current run instance.
    /// </summary>
    [RelayCommand]
    private void UpdateView(string viewName)
    {
        SelectedView = viewName;
        Refresh();
    }

    /// <summary>
    /// Opens the node associated with the current model by fetching the node information
    /// and navigating to the NodeObserver for that node.
    /// </summary>
    [RelayCommand]
    private async Task OpenNode()
    {
        var result = await Mediator.Send(new GetNode(Model.Node.NodeId));
        if (Notifier.ShowIfFailed(result)) return;
        await Navigator.Navigate(new NodeObserver(result.Value));
    }

    /// <summary>
    /// Expands the current Run and all its child Runs recursively.
    /// </summary>
    [RelayCommand]
    public void ExpandAll()
    {
        IsExpanded = true;

        foreach (var run in Runs)
        {
            run.ExpandAll();
        }
    }

    /// <summary>
    /// Collapses the current Run and recursively collapses all child Runs.
    /// </summary>
    [RelayCommand]
    public void CollapseAll()
    {
        IsExpanded = false;

        foreach (var run in Runs)
        {
            run.CollapseAll();
        }
    }

    /// <summary>
    /// When the underlying result state changes, trigger binding refresh.
    /// </summary>
    public void Receive(StateChange message)
    {
        if (message.Run.RunId != Model.RunId) return;
        Verifications.Refresh();
        Evaluations.Refresh();
        Refresh();
    }

    /// <summary>
    /// When the selected filter state changes refresh the visible evaluations.
    /// </summary>
    partial void OnFilterStateChanged(ResultState value)
    {
        Evaluations.Filter(x => value == ResultState.None || x.Result == value);
        Verifications.Filter(x => value == ResultState.None || x.Result == value);
    }

    private IEnumerable<ResultState> GetDistinctResultStates()
    {
        return new[] { ResultState.None }.Concat(Runs.SelectMany(r => r.Model.DistinctResults()).Distinct());
    }

    /// <summary>
    /// A message that is sent to notify the result observer to update or refresh the state of the result.
    /// </summary>
    public record StateChange(Run Run);

    /// <summary>
    /// A message that will initiate/execute a run for this instance. 
    /// </summary>
    public record ExecuteRun(RunObserver Run);

    public static implicit operator Run(RunObserver observer) => observer.Model;
    public static implicit operator RunObserver(Run model) => new(model);
}