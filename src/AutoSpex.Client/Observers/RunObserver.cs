using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Observers;

public partial class RunObserver : Observer<Run>
{
    /// <inheritdoc/>
    public RunObserver(Run model) : base(model)
    {
        Result = Model.Result;
        Nodes = new ObserverCollection<Node, NodeObserver>(
            refresh: () => Model.Nodes.Select(n => new NodeObserver(n)).ToList(),
            add: (_, n) => Model.AddNode(n),
            remove: (_, n) => Model.RemoveNode(n),
            clear: () => Model.Clear()
        );

        Outcomes = new ObserverCollection<Outcome, OutcomeObserver>(() =>
            Model.Outcomes.Select(x => new OutcomeObserver(x)).ToList());

        Track(Nodes);
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
    [ObservableProperty] private ResultState _result;
    public DateTime RanOn => Model.RanOn;
    public string RanBy => Model.RanBy;
    public int Total => Model.Sources.Count() * Model.Specs.Count();
    public int Ran => Outcomes.Count;
    public int Passed => Outcomes.Count(x => x.Result == ResultState.Passed);
    public int Failed => Outcomes.Count(x => x.Result == ResultState.Failed);
    public int Error => Outcomes.Count(x => x.Result == ResultState.Error);
    public long Duration => Outcomes.Sum(x => x.Duration);
    public long Average => Outcomes.Count > 0 ? Outcomes.Sum(x => x.Duration) / Outcomes.Count : 0;
    public ObserverCollection<Node, NodeObserver> Nodes { get; }
    public ObserverCollection<Outcome, OutcomeObserver> Outcomes { get; }

    [ObservableProperty] private bool _isVirtual;

    /// <summary>
    /// A static factory method that creates a new virtual run observer with the provided seed node. It also outputs the
    /// created run node.
    /// </summary>
    /// <param name="seed">The node to seed this run with.</param>
    /// <param name="node">The run node that was created.</param>
    /// <returns>A new <see cref="RunObserver"/> wrapping the run.</returns>
    public static RunObserver Virtual(NodeObserver seed, out NodeObserver node)
    {
        node = Node.NewRun();
        return new RunObserver(node, seed);
    }

    /// <summary>
    /// Adds the provided node (or its descendants) to this run configuration.
    /// </summary>
    /// <param name="node">The node to add.</param>
    public void AddNode(NodeObserver node)
    {
        //Getting a local node to use as the different observer instance to respect different UI properties.
        var observer = new NodeObserver(node);
        //We have to add to the model since it handles adding only descendants and not the container node itself (ObserverCollection doesn't know not to).
        //So after each add we just need to Sync the collection of the underlying model to our ObserverCollection. (Sync will trigger change notification which we want)
        Model.AddNode(observer);
        Nodes.Sync();
        Outcomes.Refresh();
    }

    /// <summary>
    /// Triggers the execution of this run from the runner page using the StartRun message.
    /// </summary>
    public void TriggerRun() => Messenger.Send(new StartRun(this));


    [RelayCommand]
    public async Task Execute(CancellationToken token = default)
    {
        var specs = (await LoadSpecs(token)).ToList();
        var sources = Model.Sources.Select(n => n.NodeId).ToList();

        foreach (var id in sources)
        {
            await RunSource(id, specs, token);
        }

        Refresh();
    }

    /// <summary>
    /// Runs the source against all provided specifications.
    /// This method will lead the source from the database and then run each spec, updating the page as processing ocurrs.
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
    }

    public static implicit operator Run(RunObserver observer) => observer.Model;
    public static implicit operator RunObserver(Run model) => new(model);

    /// <summary>
    /// A messages that is sent to trigger the start of the provided <see cref="RunObserver"/>.
    /// </summary>
    /// <param name="Run">The run to start running.</param>
    public record StartRun(RunObserver Run);
}