using System;
using System.Linq;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.ComponentModel;

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
            clear: () => Model.ClearNodes()
        );
        
        Outcomes = new ObserverCollection<Outcome, OutcomeObserver>(
            refresh: () => Model.Outcomes.Select(x => new OutcomeObserver(x)).ToList(),
            add: (_, m) => Model.AddOutcome(m),
            remove: (_, m) => Model.RemoveOutcome(m),
            clear: () => Model.ClearOutcomes()
        );
        
        Track(Nodes);
        Track(Outcomes);
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
    }

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

    public static implicit operator Run(RunObserver observer) => observer.Model;
    public static implicit operator RunObserver(Run model) => new(model);
}