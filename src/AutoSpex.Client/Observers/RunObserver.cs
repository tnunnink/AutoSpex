using System;
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

public partial class RunObserver : Observer<Run>, IRecipient<Observer.Get<RunObserver>>
{
    private CancellationTokenSource? _cancellation;
    private int Total => Model.Outcomes.Count();

    /// <inheritdoc/>
    public RunObserver(Run model) : base(model)
    {
        Result = Model.Result;
        Node = new NodeObserver(Model.Node);
        Source = new SourceObserver(Model.Source);

        Outcomes = new ObserverCollection<Outcome, OutcomeObserver>(
            refresh: () => Model.Outcomes.Select(x => new OutcomeObserver(x)).ToList(),
            count: () => Model.Outcomes.Count()
        );

        RegisterDisposable(Node);
        RegisterDisposable(Source);
        RegisterDisposable(Outcomes);
    }

    public override Guid Id => Model.RunId;
    public override string Name => Model.Name;
    public override string Icon => nameof(Run);
    public NodeObserver Node { get; }
    public SourceObserver Source { get; }
    public ObserverCollection<Outcome, OutcomeObserver> Outcomes { get; }

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(CancelCommand))]
    private ResultState _result;

    [ObservableProperty] private int _ran;

    [ObservableProperty] private ResultState _filterState = ResultState.None;

    public string Message => $"Ran on {Model.RanOn:MMM dd, yyyy hh:mm tt} by {Model.RanBy}";
    public string Duration => $"{Model.Duration} ms";
    public string PassRate => $"{Model.PassRate:F1} %";
    public float Progress => Total > 0 ? (float)Ran / Total * 100 : 0;
    public int Count => GetFilterCount();
    public IEnumerable<ResultState> States => GetDistinctStates();

    /// <inheritdoc />
    public override bool Filter(string? filter)
    {
        return base.Filter(filter) || Message.Satisfies(filter);
    }

    /// <summary>
    /// 
    /// </summary>
    public async Task ExecuteLocal()
    {
        _cancellation = new CancellationTokenSource();
        var token = _cancellation.Token;

        try
        {
            var source = await LoadSource(token);
            List<Node> nodes = [Node.Model];
            await ResolveReferences(nodes);
            await Model.Execute(nodes, source, OnSpecRunning, OnSpecCompleted, token);
        }
        catch (OperationCanceledException)
        {
            Notifier.ShowWarning("Run canceled", $"{Name} was canceled prior to finishing execution.");
        }
        catch (InvalidOperationException e)
        {
            Notifier.ShowWarning("Run failed", $"{Name} failed because {e.Message}.");
        }

        Result = Model.Result;
        OnPropertyChanged(string.Empty);
    }

    /// <summary>
    /// Command to execute this run by retrieving, resolving, and evaluating all configured spec/source pairs and
    /// producing new outcome results.
    /// </summary>
    public async Task Execute()
    {
        _cancellation = new CancellationTokenSource();
        var token = _cancellation.Token;

        MarkPending();

        //For visual effect only.
        await Task.Delay(500, token);

        try
        {
            var source = await LoadSource(token);
            var nodes = await LoadSpecs(token);
            await ResolveReferences(nodes);
            await Model.Execute(nodes, source, OnSpecRunning, OnSpecCompleted, token);
        }
        catch (OperationCanceledException)
        {
            Notifier.ShowWarning("Run canceled", $"{Name} was canceled prior to finishing execution.");
        }
        catch (InvalidOperationException e)
        {
            Notifier.ShowWarning("Run failed", $"{Name} failed because {e.Message}.");
        }

        Result = Model.Result;
        OnPropertyChanged(string.Empty);
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
    /// When the filter text for a run observer changes
    /// </summary>
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName is nameof(FilterText) or nameof(FilterState))
            ApplyFilters(FilterState, FilterText);
    }

    /// <summary>
    /// Handle requests for the run observer instance. This is used by the child outcome to get the run source id.
    /// </summary>
    public void Receive(Get<RunObserver> message)
    {
        if (message.HasReceivedResponse) return;
        if (!message.Predicate(this)) return;
        message.Reply(this);
    }

    /// <summary>
    /// Loads the full source object configured for this run. This is the source that all specs will be run against.
    /// </summary>
    private async Task<Source> LoadSource(CancellationToken token)
    {
        var sourceId = Model.Source.SourceId;
        var result = await Mediator.Send(new LoadSource(sourceId), token);

        if (result.IsFailed)
        {
            throw new InvalidOperationException(
                $"Source {Model.Source.Name} could not be loaded. Ensure source exists in application.");
        }

        return result.Value;
    }

    /// <summary>
    /// Load all specs configured for this run. This is executed prior to each run to get the most updated
    /// configuration to execute.
    /// </summary>
    private async Task<List<Node>> LoadSpecs(CancellationToken token)
    {
        var ids = Model.Outcomes.Select(n => n.NodeId);
        var nodes = await Mediator.Send(new LoadNodes(ids), token);
        return nodes.ToList();
    }

    /// <summary>
    /// Sends request to resolve all external source references declared in the provided spec configurations.
    /// </summary>
    private Task<Result> ResolveReferences(IEnumerable<Node> nodes)
    {
        return Mediator.Send(new ResolveReferences(nodes.Select(n => n.Spec)));
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

        Ran++;
        OnPropertyChanged(nameof(Progress));
    }

    /// <summary>
    /// Sets this run and all outcomes configured to a pending result state to indicate to the UI that these specs
    /// are awaiting a new result. 
    /// </summary>
    private void MarkPending()
    {
        Ran = 0;
        OnPropertyChanged(nameof(Progress));

        foreach (var outcome in Outcomes)
        {
            outcome.Result = ResultState.Pending;
        }

        Result = ResultState.Pending;
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

        OnPropertyChanged(nameof(Count));
    }

    /// <summary>
    /// Gets the count of outcomes based on the filter state.
    /// </summary>
    private int GetFilterCount()
    {
        if (FilterState == ResultState.None) return Model.Outcomes.Count();
        return Model.Outcomes.Count(x => x.Result == FilterState);
    }

    /// <summary>
    /// Gets distinct result states of the run's outcomes for filter selection.
    /// </summary>
    private List<ResultState> GetDistinctStates()
    {
        var states = new List<ResultState> { ResultState.None };
        states.AddRange(Model.Outcomes.Select(x => x.Result).Distinct());
        return states.OrderBy(r => r.Value).ToList();
    }

    public static implicit operator Run(RunObserver observer) => observer.Model;
    public static implicit operator RunObserver(Run model) => new(model);
}