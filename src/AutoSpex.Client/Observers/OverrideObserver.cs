using System;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;

namespace AutoSpex.Client.Observers;

public class OverrideObserver : Observer<Node>
{
    public OverrideObserver(Node model) : base(model)
    {
        Node = new NodeObserver(Model);
        Spec = new SpecObserver(Model.Spec);

        RegisterDisposable(Node);
        Track(Spec);
    }

    public override Guid Id => Model.NodeId;
    public override string Name => Model.Name;
    public string Route => Model.Route;
    public NodeObserver Node { get; }
    public SpecObserver Spec { get; }
    protected override bool PromptForDeletion => false;


    /// <inheritdoc />
    //todo at some point this could open the full config page for the spec instead of navigating the node.
    protected override Task Navigate() => Task.CompletedTask;

    /// <inheritdoc />
    public override bool Filter(string? filter)
    {
        FilterText = filter;
        return Node.Filter(filter);
    }

    public static implicit operator OverrideObserver(Node node) => new(node);
    public static implicit operator Node(OverrideObserver observer) => observer.Model;
}