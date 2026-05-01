using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class RunnerPageModel(NodeObserver node) : DetailPageModel(node.Name),
    IRecipient<RunObserver.ExecuteRun>
{
    private CancellationTokenSource? _cancellation;
    public override string Route => $"Run/{node.Type}/{node.Id}";
    public override string Icon => "Run";
    public ObserverCollection<Run, RunObserver> Runs { get; } = [];
    public ObservableCollection<RunObserver> SelectedRuns { get; } = [];

    [ObservableProperty] private RunObserver? _selectedRun;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(CancelCommand))]
    private ResultState _result = ResultState.None;

    [ObservableProperty] private ResultState _filterState = ResultState.None;

    [ObservableProperty] private IEnumerable<ResultState> _states = [];

    /// <inheritdoc />
    public override Task Load()
    {
        //Get all targets sources.
        var sources = GetRepoTargets();

        //Generate runs with the provided node for each source.
        var runs = sources.Select(s => new Run(node, s)).Select(r => new RunObserver(r)).ToList();
        Runs.BindReadOnly(runs);
        RegisterDisposable(Runs);

        //Upon loading, run all runs.
        return RunAll();
    }

    /// <summary>
    /// Command to run all current <see cref="Runs"/> for this runner page.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanRun))]
    private async Task RunAll()
    {
        await ExecuteRuns(Runs.Select(r => r.Model).ToArray());
    }

    /// <summary>
    /// Indicates that a run can be executed.
    /// </summary>
    private bool CanRun() => _cancellation is null && !Result.IsProcessing;

    /// <summary>
    /// Command to cancel execution of this run.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanCancel))]
    private void Cancel() => _cancellation?.Cancel();

    /// <summary>
    /// Indicates that the run can be canceled.
    /// </summary>
    private bool CanCancel() => _cancellation is not null && Result.IsProcessing;

    /// <summary>
    /// Expands all nodes in the tree. This is implemented through the node IsExpanded property.
    /// </summary>
    [RelayCommand]
    private void ExpandAll()
    {
        foreach (var run in Runs)
        {
            run.ExpandAll();
        }
    }

    /// <summary>
    /// Collapses all nodes in the tree. This is implemented through the node IsExpanded property.
    /// </summary>
    [RelayCommand]
    private void CollapseAll()
    {
        foreach (var run in Runs)
        {
            run.CollapseAll();
        }
    }

    /// <summary>
    /// Handle the message to trigger the provided run instance if the instance is on contained in the runner page.
    /// </summary>
    public void Receive(RunObserver.ExecuteRun message)
    {
        if (!Runs.Has(message.Run)) return;

        ExecuteRuns([message.Run]).Forget(e => Notifier.ShowError("Run failed", e.Message));
    }

    /// <inheritdoc />
    protected override void FilterChanged(string? filter)
    {
        Runs.Filter(r => r.FilterTree(filter, FilterState));
    }

    /// <summary>
    /// When the selected filter state changes refresh the visible evaluations.
    /// </summary>
    partial void OnFilterStateChanged(ResultState value)
    {
        Runs.Filter(r => r.FilterTree(Filter, value));
    }

    /// <summary>
    /// Executes the provided runs. Updates the state of the runner page to allow cancellation and block new runs.
    /// Wires up the state change notification message to refresh nodes as they complete.
    /// Updates the overall result once complete and posts the results to the database.
    /// </summary>
    /// <param name="runs">The collection of runs to execute.</param>
    private async Task ExecuteRuns(Run[] runs)
    {
        //Auto open the runner drawer when we start a run.
        Messenger.Send(new AppPageModel.OpenDrawerRequest());

        //Set status pending and create a new cancellation source.
        _cancellation = new CancellationTokenSource();
        Result = ResultState.Pending;

        //Execute the configured runs.
        await Runner.Run(runs, OnRunStateChanged, _cancellation.Token);

        //Update the result state for the page.
        Result = ResultState.MaxOrDefault(Runs.Select(r => r.Result).ToArray());
        States = new[] { ResultState.None }.Concat(Runs.SelectMany(r => r.Model.DistinctResults()).Distinct());

        //todo post results to database?

        _cancellation = null;
    }

    /// <summary>
    /// Send a message to notify the run observer instance of a state change to update UI bound elements.
    /// </summary>
    private void OnRunStateChanged(Run run)
    {
        Messenger.Send(new RunObserver.StateChange(run));
    }

    /// <summary>
    /// Gets all targeted sources for the current connected repo if any are available or selected.
    /// If not then we return an empty collection.
    /// </summary>
    private IEnumerable<SourceObserver> GetRepoTargets()
    {
        var message = Messenger.Send(new Observer.Get<RepoObserver>(r => r.IsConnected));

        return message.HasReceivedResponse
            ? message.Response.Targeted
            : [];
    }
}