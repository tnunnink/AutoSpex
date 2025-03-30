using System;
using System.Linq;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Observers;

public partial class ResultObserver : Observer<Node>, IRecipient<ResultObserver.ResultChange>
{
    /// <inheritdoc/>
    public ResultObserver(Node model) : base(model)
    {
        Evaluations = new ObserverCollection<Evaluation, EvaluationObserver>(
            refresh: () => Model.Result.Evaluations.Select(x => new EvaluationObserver(x)).ToList(),
            count: () => Model.Result.Evaluations.Count
        );

        Nodes = new ObserverCollection<Node, ResultObserver>(
            refresh: () => Model.Nodes.Select(x => new ResultObserver(x)).ToList(),
            count: () => Model.Nodes.Count()
        );

        RegisterDisposable(Evaluations);
        RegisterDisposable(Nodes);
    }

    protected override bool PromptForDeletion => false;
    public override Guid Id => Model.NodeId;
    public override string Name => Model.Name;
    public string SourceName => $"[{Model.Result.Source.Name}]";
    public ResultState Result => Model.Result.State;
    public string Duration => $"{Model.Result.Duration} ms";
    public ObserverCollection<Evaluation, EvaluationObserver> Evaluations { get; }
    public ObserverCollection<Node, ResultObserver> Nodes { get; }

    public int PassedCount => Evaluations.Count(e => e.Result == ResultState.Passed);
    public int FailedCount => Evaluations.Count(e => e.Result == ResultState.Failed);
    public int ErroredCount => Evaluations.Count(e => e.Result == ResultState.Errored);
    public int InconclusiveCount => Evaluations.Count(e => e.Result == ResultState.Inconclusive);

    [ObservableProperty] private ResultState _filterState = ResultState.None;
    
    /// <summary>
    /// Filters the tree structure based on the provided filter string recursively.
    /// </summary>
    /// <param name="filter">The filter string to apply.</param>
    /// <returns>True if any node in the tree structure is visible after filtering; otherwise, false.</returns>
    public bool FilterTree(string? filter)
    {
        var passes = base.Filter(filter);
        var children = Nodes.Count(x => x.FilterTree(filter));

        IsVisible = passes || children > 0;
        IsExpanded = string.IsNullOrEmpty(filter) ? IsExpanded : children > 0;

        return IsVisible;
    }

    /// <inheritdoc />
    /// <remarks>
    /// We will reuse this command to open the corresponding node instance in the editor.
    /// </remarks>
    protected override async Task Navigate()
    {
        await Navigator.Navigate(new NodeObserver(Model));
    }

    /// <summary>
    /// Sets the <see cref="FilterState"/> of this result object which will in turn filter the <see cref="Evaluations"/>
    /// based on the provided state.
    /// </summary>
    [RelayCommand]
    private void ApplyFilterState(ResultState state)
    {
        FilterState = state;
    }

    /// <summary>
    /// When the underlying result state changes, trigger binding refresh.
    /// </summary>
    public void Receive(ResultChange message)
    {
        if (message.Verification.VerificationId != Model.Result.VerificationId) return;
        Evaluations.Refresh();
        OnPropertyChanged(string.Empty);
    }

    /// <summary>
    /// When the selected filter state changes refresh the visible evaluations.
    /// </summary>
    partial void OnFilterStateChanged(ResultState value)
    {
        Evaluations.Filter(x => value == ResultState.None || x.Result == value);
    }

    /// <summary>
    /// A message that is sent to notify the result observer to update or refresh the state of the result.
    /// </summary>
    public record ResultChange(Verification Verification);

    public static implicit operator Node(ResultObserver observer) => observer.Model;
    public static implicit operator ResultObserver(Node model) => new(model);
}