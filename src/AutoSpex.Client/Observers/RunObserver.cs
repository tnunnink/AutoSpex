using System;
using System.Linq;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Observers;

public partial class RunObserver : Observer<Run>, IRecipient<RunObserver.StateChange>
{
    /// <inheritdoc/>
    public RunObserver(Run model) : base(model)
    {
        Results = new ObserverCollection<Verification, VerificationObserver>(
            refresh: () => Model.Results.Select(x => new VerificationObserver(x)).ToList(),
            count: () => Model.Results.Count
        );

        Runs = new ObserverCollection<Run, RunObserver>(
            refresh: () => Model.Runs.Select(x => new RunObserver(x)).ToList(),
            count: () => Model.Runs.Count
        );

        RegisterDisposable(Results);
        RegisterDisposable(Runs);
    }

    public override Guid Id => Model.RunId;
    public override string Name => Model.Node.Name;
    public string SourceName => $"({Model.Source.Name})";
    public ResultState Result => Model.Result;
    public string Duration => $"{Model.Duration} ms";
    public ObserverCollection<Verification, VerificationObserver> Results { get; }
    public ObserverCollection<Run, RunObserver> Runs { get; }
    public string RunCount => $"({Runs.Count})";
    public int PassedCount => Results.Count(e => e.Result == ResultState.Passed);
    public int FailedCount => Results.Count(e => e.Result == ResultState.Failed);
    public int ErroredCount => Results.Count(e => e.Result == ResultState.Errored);

    [ObservableProperty] private ResultState _filterState = ResultState.None;

    /// <summary>
    /// Filters the tree structure based on the provided filter string recursively.
    /// </summary>
    /// <param name="filter">The filter string to apply.</param>
    /// <returns>True if any node in the tree structure is visible after filtering; otherwise, false.</returns>
    public bool FilterTree(string? filter)
    {
        var passes = base.Filter(filter);
        var children = Runs.Count(x => x.FilterTree(filter));

        IsVisible = passes || children > 0;
        IsExpanded = string.IsNullOrEmpty(filter) ? IsExpanded : children > 0;

        return IsVisible;
    }

    /// <summary>
    /// Filters the <see cref="Results"/> for this run instance using the provided filer text and configured
    /// <see cref="FilterState"/>.
    /// </summary>
    /// <param name="filter">The filter text to apply.</param>
    protected void FilterResults(string? filter)
    {
        Results.Filter(x =>
        {
            var hasState = FilterState == ResultState.None || x.Result == FilterState;
            var hasText = x.Filter(filter);
            return hasState && hasText;
        });
    }

    /// <summary>
    /// Sets the <see cref="FilterState"/> of this result object which will in turn filter the <see cref="Results"/>
    /// based on the provided state.
    /// </summary>
    [RelayCommand]
    private void ApplyFilterState(ResultState state)
    {
        FilterState = state;
    }

    /// <summary>
    /// 
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
    /// 
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
        Results.Refresh();
        OnPropertyChanged(string.Empty);
    }

    /// <summary>
    /// When the selected filter state changes refresh the visible evaluations.
    /// </summary>
    partial void OnFilterStateChanged(ResultState value)
    {
        Results.Filter(x => value == ResultState.None || x.Result == value);
    }

    /// <summary>
    /// A message that is sent to notify the result observer to update or refresh the state of the result.
    /// </summary>
    public record StateChange(Run Run);

    public static implicit operator Run(RunObserver observer) => observer.Model;
    public static implicit operator RunObserver(Run model) => new(model);
}