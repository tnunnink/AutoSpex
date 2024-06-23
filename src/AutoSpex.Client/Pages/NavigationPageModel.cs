using System;
using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Pages;

public partial class NavigationPageModel(string project, NodeType feature) : PageViewModel,
    IRecipient<NodeObserver.Created>,
    IRecipient<NodeObserver.Deleted>,
    IRecipient<NodeObserver.Renamed>,
    IRecipient<NodeObserver.Moved>
{
    [ObservableProperty] private NodeType _feature = feature;
    public override string Route => $"{project}/{Feature}s";
    public override string Title => $"{Feature}s";
    public override string Icon => $"{Feature}s";

    [ObservableProperty] private ObserverCollection<Node, NodeObserver> _nodes = [];
    public ObservableCollection<NodeObserver> SelectedNodes { get; } = [];

    public override async Task Load()
    {
        var result = await Mediator.Send(new ListNodes(Feature));
        if (result.IsFailed) return;

        Nodes.Clear();
        Nodes.AddRange(result.Value.Select(n => new NodeObserver(n)));
        Nodes.Sort(n => n.Name, StringComparer.OrdinalIgnoreCase);
    }

    [RelayCommand]
    private async Task AddContainer()
    {
        var node = Node.NewContainer();
        var result = await Mediator.Send(new CreateNode(node, Feature));
        if (result.IsFailed) return;
        await AddNode(node);
    }

    [RelayCommand]
    private async Task AddItem()
    {
        var node = Node.NewNode(Feature);
        var result = await Mediator.Send(new CreateNode(node));
        if (result.IsFailed) return;
        await AddNode(node);
    }

    public void Receive(Observer<Node>.Created message)
    {
        if (message.Observer is not NodeObserver node) return;
        node.IsSelected = true;
        node.IsExpanded = true;
    }

    /// <summary>
    /// When anode is deleted from the root collection ensure that it is removed.
    /// </summary>
    public void Receive(NodeObserver.Deleted message)
    {
        if (message.Observer is not NodeObserver node) return;
        if (node.Feature != Feature) return;
        if (!Nodes.Contains(node)) return;
        Nodes.Remove(node);
    }

    /// <summary>
    /// When a node in the root collection is renamed, resort to ensure order.
    /// </summary>
    public void Receive(NodeObserver.Renamed message)
    {
        if (!Nodes.Contains(message.Node)) return;
        Nodes.Sort(n => n.Name, StringComparer.OrdinalIgnoreCase);
    }

    public void Receive(NodeObserver.Moved message)
    {
        if (!Nodes.Contains(message.Node)) return;
        Nodes.Remove(message.Node);
    }

    private async Task AddNode(Node node)
    {
        var observer = new NodeObserver(node) { IsNew = true };

        Nodes.Add(observer);
        Nodes.Sort(n => n.Name, StringComparer.OrdinalIgnoreCase);

        SelectedNodes.Clear();
        SelectedNodes.Add(observer);

        await Navigator.Navigate(observer);
    }
}