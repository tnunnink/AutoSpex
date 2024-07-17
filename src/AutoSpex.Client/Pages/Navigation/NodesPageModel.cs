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
public partial class NodesPageModel : PageViewModel,
    IRecipient<Observer.Created>,
    IRecipient<Observer.Deleted>,
    IRecipient<Observer.Renamed>,
    IRecipient<Observer.GetSelected>,
    IRecipient<NodeObserver.Moved>
{
    public override string Route => "Specs";
    public override string Title => "Specs";
    public override string Icon => "Specs";
    public ObserverCollection<Node, NodeObserver> Nodes { get; } = [];
    public ObservableCollection<NodeObserver> Selected { get; } = [];

    [ObservableProperty] private string? _filter;

    [ObservableProperty] private bool _isExpanded;

    public override async Task Load()
    {
        var result = await Mediator.Send(new ListNodes());
        if (result.IsFailed) return;

        Nodes.Refresh(result.Value.Select(n => new NodeObserver(n)));
        Nodes.Sort(n => n.Name, StringComparer.OrdinalIgnoreCase);
    }

    [RelayCommand]
    private async Task AddNode(NodeType? type)
    {
        if (type is null) return;

        var node = Node.New(type);
        var result = await Mediator.Send(new CreateNode(node));
        if (result.IsFailed) return;

        var observer = new NodeObserver(node) { IsNew = true };

        Nodes.Add(observer);
        Nodes.Sort(n => n.Name, StringComparer.OrdinalIgnoreCase);

        Selected.Clear();
        observer.Locate();

        await Navigator.Navigate(observer);
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

        Nodes.Sort(n => n.Name, StringComparer.OrdinalIgnoreCase);
        Selected.Clear();
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

        foreach (var observer in Selected)
            message.Reply(observer);
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