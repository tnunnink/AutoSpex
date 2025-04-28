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
using NLog;
using NLog.Targets;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class RunnerPageModel(NodeObserver node) : DetailPageModel(node.Name)
{
    public override string Route => $"Run/{node.Type}/{node.Id}";
    public override string Icon => "Run";

    private CancellationTokenSource? _cancellation;
    public ObserverCollection<Run, RunObserver> Runs { get; } = [];
    public ObservableCollection<RunObserver> SelectedRuns { get; } = [];

    [ObservableProperty] private RunObserver? _selectedRun;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(CancelCommand))]
    private ResultState _result = ResultState.None;

    [ObservableProperty] private ResultState _filterState = ResultState.None;
    public ObservableCollection<string> Logs => GetCurrentLogs();

    private static ObservableCollection<string> GetCurrentLogs()
    {
        var target = LogManager.Configuration.FindTargetByName<MemoryTarget>("MemoryLog");
        return new ObservableCollection<string>(target.Logs);
    }

    /// <inheritdoc />
    public override Task Load()
    {
        //Get all targets sources.
        var sources = GetRepoTargets();

        //Generate runs with the provided node for each source.
        var runs = sources.Select(s => new Run(node, s)).ToList();
        Runs.Bind(runs, r => new RunObserver(r));
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
        //Auto open the runner drawer when we start a run.
        Messenger.Send(new AppPageModel.OpenDrawerRequest());

        //Set status pending and create a new cancellation source.
        _cancellation = new CancellationTokenSource();
        Result = ResultState.Pending;

        //Execute the configured runs.
        var results = await Runner.Run(Runs.Select(r => r.Model).ToArray(), OnRunStateChanged, _cancellation.Token);

        //Update the result state for the page.
        Result = ResultState.MaxOrDefault(results.Select(r => r.Result).ToArray());
        _cancellation = null;
        
        OnPropertyChanged(nameof(Logs));

        //todo post results to database?
    }

    /// <summary>
    /// Command to run all current <see cref="Runs"/> for this runner page.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanRun))]
    private async Task RunSelected()
    {
        //Auto open the runner drawer when we start a run.
        Messenger.Send(new AppPageModel.OpenDrawerRequest());

        //Set status pending and create a new cancellation source.
        _cancellation = new CancellationTokenSource();
        Result = ResultState.Pending;

        //Execute the configured runs.
        var results = await Runner.Run(Runs.Select(r => r.Model).ToArray(), OnRunStateChanged, _cancellation.Token);

        //Update the result state for the page.
        Result = ResultState.MaxOrDefault(results.Select(r => r.Result).ToArray());
        _cancellation = null;

        //todo post results to database?
    }

    /// <summary>
    /// Indicates that a run can be executed.
    /// </summary>
    private bool CanRun() => _cancellation is null && Result != ResultState.Pending;

    /// <summary>
    /// Command to cancel execution of this run.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanCancel))]
    private void Cancel() => _cancellation?.Cancel();

    /// <summary>
    /// Indicates that the run can be canceled.
    /// </summary>
    private bool CanCancel() => _cancellation is not null && Result == ResultState.Pending;

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