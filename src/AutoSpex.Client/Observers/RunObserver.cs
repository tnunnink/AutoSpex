using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace AutoSpex.Client.Observers;

public partial class RunObserver : Observer<Run>
{
    private CancellationTokenSource? _cancellation;

    /// <inheritdoc/>
    public RunObserver(Run model) : base(model)
    {
        Result = Model.Result;

        Outcomes = new ObserverCollection<Outcome, OutcomeObserver>(
            refresh: () => Model.Outcomes.Select(m => new OutcomeObserver(m)).ToList(),
            add: (_, m) => Model.AddOutcome(m),
            remove: (_, m) => Model.RemoveOutcome(m),
            clear: () => Model.Clear()
        );

        Sources = new ObserverCollection<Node, NodeObserver>(
            () => Model.Sources.Select(x => new NodeObserver(x)).ToList());
        Sources.ItemPropertyChanged += OnSourcesItemChanged;

        Track(Outcomes);
        Track(nameof(Result));
        Track(nameof(RanOn));
        Track(nameof(RanBy));
    }

    /// <summary>
    /// Creates a virtual <see cref="RunObserver"/> with the provided run node and seed node.
    /// </summary>
    /// <param name="node">The node representing the run node. This is used to copy the id and name to the new <see cref="Run"/>.</param>
    /// <param name="seed">The node used to seed the run with the nodes to run.</param>
    private RunObserver(NodeObserver node, NodeObserver seed) : this(new Run(node, seed))
    {
        IsVirtual = true;
    }

    public override Guid Id => Model.RunId;
    public string Name => Model.Name;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(CancelCommand))]
    private ResultState _result;

    [ObservableProperty] private bool _isVirtual;
    public bool HasResult => Result > ResultState.Pending;
    public DateTime RanOn => Model.RanOn;
    public string RanBy => Model.RanBy;
    public int Ran => Outcomes.Count(x => x.Result > ResultState.Pending);
    public float Progress => Outcomes.Count > 0 ? (float)Ran / Outcomes.Count * 100 : 0;
    public int Passed => Outcomes.Count(x => x.Result == ResultState.Passed);
    public int Failed => Outcomes.Count(x => x.Result == ResultState.Failed);
    public int Error => Outcomes.Count(x => x.Result == ResultState.Error);
    public long Duration => Outcomes.Sum(x => x.Duration);
    public long Average => Outcomes.Count > 0 ? Outcomes.Sum(x => x.Duration) / Outcomes.Count : 0;
    public ObserverCollection<Outcome, OutcomeObserver> Outcomes { get; }
    public ObservableCollection<OutcomeObserver> Selected { get; } = [];
    public ObserverCollection<Node, NodeObserver> Sources { get; }

    [ObservableProperty] private List<NodeObserver> _sourceFilter = [];

    [ObservableProperty] private ResultState _resultFilter = ResultState.None;


    /// <summary>
    /// A static factory method that creates a new virtual run observer with the provided seed node. It also outputs the
    /// created run node.
    /// </summary>
    /// <param name="seed">The node to seed this run with.</param>
    /// <returns>A new <see cref="RunObserver"/> wrapping the run.</returns>
    public static RunObserver Virtual(NodeObserver seed)
    {
        var node = Node.NewRun();
        return new RunObserver(node, seed);
    }

    /// <summary>
    /// Adds the provided node (or its descendants) to this run configuration.
    /// </summary>
    /// <param name="node">The node to add.</param>
    [RelayCommand]
    public void AddNode(NodeObserver node)
    {
        //Getting a local node to use as the different observer instance to respect different UI properties.
        var observer = new NodeObserver(node);
        //We have to add to the model since it handles adding only descendants and not the container node itself (ObserverCollection doesn't know not to).
        //So after each add we just need to Sync the collection of the underlying model to our ObserverCollection. (Sync will trigger change notification which we want)
        Model.AddNode(observer);
        Outcomes.Sync();
        Sources.Sync();
        RunCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// Sends a message to open this run in the runner page.
    /// </summary>
    [RelayCommand]
    public async Task Open()
    {
        var message = new OpenRun(this);
        await Messenger.Send(message);
    }

    /// <summary>
    /// Sends the message to close this run observer from the containing runner.
    /// </summary>
    [RelayCommand]
    private void Close()
    {
        var message = new CloseRun(this);
        Messenger.Send(message);
        IsActive = false;
    }

    /// <summary>
    /// Updates the local result filter property to allow the UI to update the displayed list of outcomes based on the
    /// selected result.
    /// </summary>
    /// <param name="result"></param>
    [RelayCommand]
    private void ApplyResultFilter(ResultState result)
    {
        ResultFilter = result;
    }

    /// <summary>
    /// Command to execute this run by retrieving, resolving, and evaluating all configured spec/source pairs and
    /// producing new outcome results.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanRun))]
    private async Task Run()
    {
        _cancellation = new CancellationTokenSource();
        var token = _cancellation.Token;

        MarkPending();

        try
        {
            var specs = (await LoadSpecs(token)).ToList();
            var sources = Model.Sources.Select(n => n.NodeId).ToList();

            foreach (var id in sources)
            {
                await RunSource(id, specs, token);
            }
        }
        catch (OperationCanceledException)
        {
        }

        Result = Model.Result;
        Refresh();
    }

    /// <summary>
    /// Indicates that this run can be executed.
    /// </summary>
    private bool CanRun() => Model.Specs.Any() && Model.Sources.Any();

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
    /// Runs the source against all provided specifications.
    /// This method will lead the source from the database and then run each spec, updating the page as processing occurs.
    /// </summary>
    private async Task RunSource(Guid id, IEnumerable<Spec> specs, CancellationToken token)
    {
        var source = await LoadSource(id, token);
        if (source is null) return;
        await Model.Execute(source, specs, OnOutcomeRunning, OnOutcomeCompleted, token);
    }

    /// <summary>
    /// Load all specs configured for the provided <see cref="RunObserver"/>.
    /// </summary>
    private async Task<IEnumerable<Spec>> LoadSpecs(CancellationToken token)
    {
        var ids = Model.Specs.Select(n => n.NodeId);
        var result = await Mediator.Send(new LoadSpecs(ids), token);
        return result.IsSuccess ? result.Value : Enumerable.Empty<Spec>();
    }

    /// <summary>
    /// Load all sources
    /// </summary>
    private async Task<Source?> LoadSource(Guid sourceId, CancellationToken token)
    {
        var result = await Mediator.Send(new GetSource(sourceId), token);
        return result.IsSuccess ? result.Value : default;
    }

    /// <summary>
    /// Sends a message that the provided outcome is starting to run.
    /// </summary>
    /// <param name="outcome">The outcome that is about to be run.</param>
    private void OnOutcomeRunning(Outcome outcome)
    {
        var message = new OutcomeObserver.Running(outcome.OutcomeId);
        Messenger.Send(message);
    }

    /// <summary>
    /// Sends a message that the provided outcome has completed running and its state is updated with the result.
    /// </summary>
    /// <param name="outcome">The outcome that just ran.</param>
    private void OnOutcomeCompleted(Outcome outcome)
    {
        var message = new OutcomeObserver.Complete(outcome);
        Messenger.Send(message);
        OnPropertyChanged(nameof(Ran));
        OnPropertyChanged(nameof(Progress));
    }

    /// <summary>
    /// Sets this run and all outcomes configured to a pending result state to indicate to the UI that these specs/sources
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
    /// When a child source node is checked changes we want to refresh out local SourceFilter collection to update the
    /// UI with the filtered set of selected sources.
    /// </summary>
    private void OnSourcesItemChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(NodeObserver.IsChecked))
        {
            SourceFilter = Sources.Where(x => x.IsChecked).ToList();
        }
    }

    public static implicit operator Run(RunObserver observer) => observer.Model;
    public static implicit operator RunObserver(Run model) => new(model);

    /// <summary>
    /// A messages that is sent to trigger the start of the provided <see cref="RunObserver"/>.
    /// </summary>
    /// <param name="run">The run to start running.</param>
    public class OpenRun(RunObserver run) : AsyncRequestMessage<bool>
    {
        /// <summary>
        /// The run to start running.
        /// </summary>
        public RunObserver Run { get; } = run;
    }

    public record CloseRun(RunObserver Run);
}