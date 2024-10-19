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
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class NodeTreePageModel : PageViewModel,
    IRecipient<Observer.Created>,
    IRecipient<Observer.Deleted>,
    IRecipient<Observer.Renamed>,
    IRecipient<Observer.GetSelected>,
    IRecipient<NodeObserver.Moved>
{
    public ObserverCollection<Node, NodeObserver> Nodes { get; } = [];
    public ObservableCollection<NodeObserver> Selected { get; } = [];

    [ObservableProperty] private string? _filter;

    [ObservableProperty] private bool _isExpanded;

    public override async Task Load()
    {
        var result = await Mediator.Send(new ListNodes());

        Nodes.Refresh(result.Select(n => new NodeObserver(n)));
        Nodes.Sort(n => n.Name, StringComparer.OrdinalIgnoreCase);
        RegisterDisposable(Nodes);
    }

    /// <summary>
    /// Expands all nodes in the tree. This is implemented through the node IsExpanded property.
    /// </summary>
    [RelayCommand]
    private void ExpandAll()
    {
        foreach (var node in Nodes)
        {
            node.ExpandAll();
        }

        //Indicate we have expanded all and toggles the state to the collapse button.
        IsExpanded = true;
    }

    /// <summary>
    /// Collapses all nodes in the tree. This is implemented through the node IsExpanded property.
    /// </summary>
    [RelayCommand]
    private void CollapseAll()
    {
        foreach (var node in Nodes)
        {
            node.CollapseAll();
        }

        //Indicate we have collapsed all and toggles the state to the expand button.
        IsExpanded = false;
    }

    /// <summary>
    /// If a collection node is created elsewhere we need to sort and locate it in the tree view.
    /// </summary>
    public void Receive(Observer.Created message)
    {
        if (message.Observer is not NodeObserver node) return;
        if (node.ParentId != Guid.Empty) return;

        Nodes.Add(new NodeObserver(node));
        Nodes.Sort(n => n.Name, StringComparer.OrdinalIgnoreCase);
        node.Locate();
    }

    /// <summary>
    /// When a node is deleted from the root collection ensure that it is removed.
    /// </summary>
    public void Receive(Observer.Deleted message)
    {
        if (message.Observer is not NodeObserver node) return;
        Nodes.Remove(node);
    }

    /// <summary>
    /// When a node in the root collection is renamed, resort to ensure order.
    /// </summary>
    public void Receive(Observer.Renamed message)
    {
        if (message.Observer is not NodeObserver node) return;
        if (!Nodes.Contains(node)) return;
        Nodes.Sort(n => n.Name, StringComparer.OrdinalIgnoreCase);
    }

    public void Receive(NodeObserver.Moved message)
    {
        Nodes.RemoveAny(n => n.ParentId != Guid.Empty);
        Nodes.Sort(n => n.Name, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// When a selection request is sent we respond with all selected nodes in this page. This page will hold all
    /// node instances for the user.
    /// </summary>
    public void Receive(Observer.GetSelected message)
    {
        if (message.Observer is not NodeObserver) return;
        if (!Selected.Any(x => x.Is(message.Observer))) return;

        foreach (var observer in Selected)
        {
            message.Reply(observer);
        }
    }

    /// <summary>
    /// When the filter text changes apply the nodes filter function to filter the tree view.
    /// </summary>
    partial void OnFilterChanged(string? value)
    {
        foreach (var node in Nodes)
        {
            node.Filter(value);
        }
    }
}