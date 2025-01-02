using System;
using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class NodeSelectorPageModel(Action<NodeObserver?> onSelected) : PageViewModel
{
    public override bool Reload => true;
    public ObserverCollection<Node, NodeObserver> Nodes { get; } = [];

    /// <inheritdoc />
    public override async Task Load()
    {
        var collections = await Mediator.Send(new ListNodes());
        var nodes = collections.SelectMany(n => n.DescendantsAndSelf()).Select(n => new NodeObserver(n));
        Nodes.BindReadOnly(nodes.ToList());
        RegisterDisposable(Nodes);
    }

    [RelayCommand]
    private void SelectNode(NodeObserver? node)
    {
        onSelected.Invoke(node);
    }

    /// <inheritdoc />
    protected override void FilterChanged(string? filter)
    {
        Nodes.Filter(filter);
    }
}