using System;
using System.Collections.Generic;
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
    IRecipient<Observer.Created<NodeObserver>>,
    IRecipient<Observer.Deleted>,
    IRecipient<Observer.Renamed>,
    IRecipient<Observer.GetSelected>,
    IRecipient<Observer.Get<NodeObserver>>,
    IRecipient<Observer.Find<NodeObserver>>
{
    public ObserverCollection<Node, NodeObserver> Nodes { get; } = [];
    public ObservableCollection<NodeObserver> Selected { get; } = [];

    [ObservableProperty] private bool _isExpanded;

    public override async Task Load()
    {
        var result = await Mediator.Send(new ListNodes());
        Nodes.Bind(result.ToList(), n => new NodeObserver(n));
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
    /// If a node is created elsewhere, we need to add to and sort the corresponding tree collection.
    /// </summary>
    public void Receive(Observer.Created<NodeObserver> message)
    {
        var collection = FindCollectionContaining(message.Observer);
        collection.Add(message.Observer);
        collection.Sort(n => n.Name, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// When a root collection node is deleted, ensure that it is removed locally.
    /// </summary>
    public void Receive(Observer.Deleted message)
    {
        if (message.Observer is not NodeObserver node) return;
        if (node.ParentId != Guid.Empty) return;
        Nodes.Remove(node);
    }

    /// <summary>
    /// When a root collection node is renamed, resort to ensure order.
    /// </summary>
    public void Receive(Observer.Renamed message)
    {
        if (message.Observer is not NodeObserver node) return;
        if (!Nodes.Contains(node)) return;
        Nodes.Sort(n => n.Name, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// When a selection request is sent we respond with all selected nodes in this page.
    /// This page will hold all node instances in the app. However, we also check that the selected collectionc contains
    /// the same instance that requested the selection, since there could be multiple collections in the UI that could
    /// respond to this message.
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
    /// Handles the request to get the first in memory node that passes the provided prediate condition.
    /// Since the node tree contains all nodes in the app, we only handle this message here and not from node itself,
    /// because there could be many instances of the "same" node alive in the app.
    /// </summary>
    public void Receive(Observer.Get<NodeObserver> message)
    {
        BroadcastNode(Nodes, message);
        return;

        void BroadcastNode(IEnumerable<NodeObserver> nodes, Observer.Get<NodeObserver> msg)
        {
            foreach (var node in nodes)
            {
                if (msg.Predicate(node))
                {
                    msg.Reply(new NodeObserver(node));
                    return;
                }

                BroadcastNode(node.Nodes, msg);
            }
        }
    }

    /// <summary>
    /// Handles the request to find in memory node instances that passes the provided predicate condition.
    /// Since the node tree contains all nodes in the app, we only handle this message here and not from node itself,
    /// because there could be many instances of the "same" node alive in the app.
    /// </summary>
    public void Receive(Observer.Find<NodeObserver> message)
    {
        BroadcastNodes(Nodes, message);
        return;

        void BroadcastNodes(IEnumerable<NodeObserver> nodes, Observer.Find<NodeObserver> msg)
        {
            foreach (var node in nodes)
            {
                if (msg.Predicate(node))
                {
                    msg.Reply(new NodeObserver(node));
                }

                BroadcastNodes(node.Nodes, msg);
            }
        }
    }

    /// <inheritdoc />
    protected override void FilterChanged(string? filter)
    {
        Nodes.Filter(x => x.FilterTree(filter));
    }

    /// <summary>
    /// Find the descendant node collection in which we need to add the target created node to.
    /// </summary>
    private ObserverCollection<Node, NodeObserver> FindCollectionContaining(NodeObserver target)
    {
        if (target.ParentId == Guid.Empty)
            return Nodes;

        foreach (var node in Nodes)
        {
            var parent = node.FindParentTo(target);
            if (parent is null) continue;
            return parent.Nodes;
        }

        return [];
    }
}