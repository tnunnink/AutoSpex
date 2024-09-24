using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class RunDetailPageModel : DetailPageModel, IRecipient<Observer.GetSelected>
{
    private CancellationTokenSource? _cancellation;

    public RunDetailPageModel(RunObserver run) : base(run.Name)
    {
        Run = run;
        Result = Run.Result;

        Outcomes = new ObserverCollection<Outcome, OutcomeObserver>(
            refresh: () => Run.Model.Outcomes.Select(x => new OutcomeObserver(x)).ToList(),
            count: () => Run.Model.Outcomes.Count()
        );
        Track(Outcomes, false);

        FilterPage = new ResultFilterPageModel(this);
    }

    public override string Route => $"{nameof(Run)}/{Run.Id}";
    public override string Icon => nameof(Run);
    public RunObserver Run { get; }
    public ObserverCollection<Outcome, OutcomeObserver> Outcomes { get; }
    public ObservableCollection<OutcomeObserver> Selected { get; } = [];
    public int Ran => Outcomes.Count(x => x.Result > ResultState.Pending);
    public float Progress => Outcomes.Count > 0 ? (float)Ran / Outcomes.Count * 100 : 0;
    public bool IsExpanded => Outcomes.All(x => x.IsExpanded);

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(CancelCommand))]
    private ResultState _result;

    public ResultFilterPageModel FilterPage { get; private set; }


    /// <inheritdoc />
    /// <remarks>
    /// For a run we expect the loaded run observer instance, and upon loading we should execute the run.
    /// </remarks>
    public override async Task Load()
    {
        if (CanExecute())
        {
            await Execute();
        }
    }

    /// <summary>
    /// Command to execute this run by retrieving, resolving, and evaluating all configured spec/source pairs and
    /// producing new outcome results.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExecute))]
    private async Task Execute()
    {
        _cancellation = new CancellationTokenSource();
        var token = _cancellation.Token;

        MarkPending();

        try
        {
            var nodes = (await LoadNodes(token)).ToList();
            await Run.Model.Execute(nodes, OnSpecRunning, OnSpecCompleted, token);
        }
        catch (OperationCanceledException)
        {
            Notifier.ShowWarning("Run canceled", $"{Run.Name} was canceled prior to finishing execution.");
        }

        Result = Run.Model.Result;
        Run.Refresh();
    }

    /// <summary>
    /// Indicates that this run can be executed.
    /// </summary>
    private bool CanExecute() => Outcomes.Any();

    /// <summary>
    /// Command to cancel execution of this run.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanCancel))]
    private void Cancel() => _cancellation?.Cancel();

    /// <summary>
    /// Indicates that the run can be cancelled.
    /// </summary>
    private bool CanCancel() => Run.Result == ResultState.Pending;

    /// <summary>
    /// Adds the provided node (or its descendants) to this run configuration.
    /// </summary>
    /// <param name="node">The node to add.</param>
    [RelayCommand]
    private void AddNode(NodeObserver? node)
    {
        if (node is null) return;

        //Getting a local node to use as the different observer instance to respect different UI properties.
        var observer = new NodeObserver(node);

        //We have to add to the model since it handles adding only descendants and not the container node itself.
        //So after each add we just need to Sync the collection of the underlying model to our ObserverCollection. (Sync will trigger change notification which we want)
        Run.Model.AddNode(observer);
        Outcomes.Sync();

        ExecuteCommand.NotifyCanExecuteChanged();
        SaveCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// Command to expand all visible outcome observers in the detail view.
    /// </summary>
    [RelayCommand]
    private void ExpandAll()
    {
        foreach (var outcome in Outcomes)
        {
            outcome.IsExpanded = true;
        }

        OnPropertyChanged(nameof(IsExpanded));
    }

    /// <summary>
    /// Command to collapse all visible outcome observers in the detail view.
    /// </summary>
    [RelayCommand]
    private void CollapseAll()
    {
        foreach (var outcome in Outcomes)
        {
            outcome.IsExpanded = false;
        }

        OnPropertyChanged(nameof(IsExpanded));
    }

    /// <inheritdoc />
    public override void Receive(Observer.Deleted message)
    {
        if (message.Observer is OutcomeObserver observer && Outcomes.Any(x => x.Is(observer)))
        {
            var ids = Selected.Select(x => x.Model.NodeId).ToList();
            Run.Model.RemoveNodes(ids);
            Outcomes.Refresh();
            return;
        }

        base.Receive(message);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    public void Receive(Observer.GetSelected message)
    {
        if (message.Observer is not OutcomeObserver observer) return;
        if (!Outcomes.Any(x => x.Is(observer))) return;

        foreach (var outcome in Selected)
        {
            message.Reply(outcome);
        }
    }

    /// <summary>
    /// Load all specs configured for the provided <see cref="RunObserver"/>. This is executed prior to each run to
    /// get the most updated configuration to execute.
    /// </summary>
    private async Task<IEnumerable<Node>> LoadNodes(CancellationToken token)
    {
        var ids = Run.Model.Outcomes.Select(n => n.NodeId);
        var result = await Mediator.Send(new LoadNodes(ids), token);
        return result.IsSuccess ? result.Value : [];
    }

    /// <summary>
    /// Sends a message that the provided outcome has started running so that the observer can update it's visual state.
    /// </summary>
    /// <param name="outcome">The outcoma that is about to run.</param>
    private void OnSpecRunning(Outcome outcome)
    {
        var message = new OutcomeObserver.Running(outcome.NodeId);
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
}