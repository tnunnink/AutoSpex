﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class RunnerPageModel : PageViewModel, IRecipient<RunnerObserver.Run>
{
    private readonly List<ResultObserver> _nodes = [];
    private CancellationTokenSource? _cancellation;
    private int _total;
    private int _ran;

    public RunnerPageModel()
    {
        Nodes = new ObserverCollection<Node, ResultObserver>(
            refresh: () => _nodes,
            count: () => _nodes.Count
        );
        RegisterDisposable(Nodes);
    }

    public Task<SourceTargetPageModel> SourceSelector => Navigator.Navigate<SourceTargetPageModel>();
    public ResultPageModel? ResultPage { get; private set; }

    public override async Task Load()
    {
        ResultPage = await Navigator.Navigate<ResultPageModel>();
        RegisterDisposable(ResultPage);
    }

    public ObserverCollection<Node, ResultObserver> Nodes { get; }

    [ObservableProperty] private ResultObserver? _selected;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(CancelCommand))]
    private ResultState _result = ResultState.None;

    [ObservableProperty] private ResultState _filterState = ResultState.None;

    [ObservableProperty] private string? _runningSource;
    public float Progress => _ran / (float)_total * 100;


    [RelayCommand(CanExecute = nameof(CanRun))]
    private Task RunAll()
    {
        return Task.CompletedTask;
    }

    [RelayCommand(CanExecute = nameof(CanRun))]
    private Task RunSource()
    {
        return Task.CompletedTask;
    }

    [RelayCommand(CanExecute = nameof(CanRun))]
    private Task RunNode()
    {
        return Task.CompletedTask;
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
    /// Indicates that the run can be cancelled.
    /// </summary>
    private bool CanCancel() => _cancellation is not null && Result == ResultState.Pending;

    /// <summary>
    /// When we recieve a message to run a specific node, configure the runner and start the execution.
    /// </summary>
    public async void Receive(RunnerObserver.Run message)
    {
        if (Result == ResultState.Pending)
        {
            Notifier.ShowWarning(
                "Run already processing",
                "Please stop or wait for the run to complete before starting a new run."
            );
            return;
        }

        await ExecuteRunner(message.Runner.Model);
    }

    /// <summary>
    /// Update the selected result ....
    /// </summary>
    partial void OnSelectedChanged(ResultObserver? value)
    {
        if (ResultPage is null) return;
        ResultPage.Result = value;
    }

    /// <inheritdoc />
    protected override void FilterChanged(string? filter)
    {
        Nodes.Filter(x => x.FilterTree(filter));
    }

    /// <summary>
    /// Command to execute this run by retrieving, resolving, and evaluating all configured spec/source pairs and
    /// producing new outcome results.
    /// </summary>
    private async Task ExecuteRunner(RunContext runContext)
    {
        _cancellation = new CancellationTokenSource();

        //Update UI to show progress start
        ExecutionStarting();

        try
        {
           
        }
        catch (OperationCanceledException)
        {
            Notifier.ShowWarning("Run canceled", "The current run was canceled prior to finishing execution.");
        }

        Result = ResultState.MaxOrDefault(Nodes.Select(r => r.Result).ToArray());
        OnPropertyChanged(string.Empty);
    }

    private void OnRunStarting(Source source)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            RunningSource = source.Name;
            Nodes.Refresh();
        });
    }

    /// <summary>
    /// Sends a message that the provided node has completed running and its state is updated with the result.
    /// </summary>
    private void OnNodeStateChanged(Verification verification)
    {
        Messenger.Send(new ResultObserver.ResultChange(verification));

        Dispatcher.UIThread.Invoke(() =>
        {
            _ran++;
            OnPropertyChanged(nameof(Progress));
        });
    }

    /// <summary>
    /// Prepare the UI for the start of execution of the current runner.
    /// </summary>
    private void ExecutionStarting()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            Nodes.Clear();
            Result = ResultState.Pending;
            _ran = 0;
            OnPropertyChanged(nameof(Progress));
        });
    }
}