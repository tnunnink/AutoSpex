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
public partial class NodeTreePageModel() : PageViewModel("Specs"),
    IRecipient<Observer.Created<NodeObserver>>,
    IRecipient<Observer.Deleted>,
    IRecipient<Observer.Renamed>,
    IRecipient<Observer.GetSelected>,
    IRecipient<Observer.Get<NodeObserver>>
{
    public override string Icon => "Specs";
    public ObserverCollection<Node, NodeObserver> Nodes { get; } = [];
    public ObservableCollection<NodeObserver> Selected { get; } = [];

    [ObservableProperty] private bool _isExpanded;

    public override async Task Load()
    {
        var nodes = await Mediator.Send(new ListNodes());
        Nodes.Bind(nodes.ToList(), n => new NodeObserver(n));
        RegisterDisposable(Nodes);
    }

    /// <summary>
    /// Command to quickly create a new collection node.
    /// </summary>
    [RelayCommand]
    private async Task AddCollection()
    {
        var node = Node.NewCollection();

        var result = await Mediator.Send(new CreateNode(node));
        if (Notifier.ShowIfFailed(result)) return;

        var observer = new NodeObserver(node) { IsNew = true };
        Messenger.Send(new Observer.Created<NodeObserver>(observer));
        await Navigator.Navigate(observer);
    }

    /// <summary>
    /// Command to import new package into the application.
    /// </summary>
    [RelayCommand]
    private async Task Import()
    {
        var package = await Prompter.Show<Package?>(() => new OpenPackagePageModel());
        if (package is null) return;

        var action = ImportAction.None;
        var exists = await Mediator.Send(new ContainsNode(package.Collection.Name, NodeType.Collection));

        if (exists)
        {
            action = await Prompter.Show<ImportAction?>(() => new ImportConflictPageModel(package));
        }

        if (action is null || action == ImportAction.Cancel) return;

        var import = await Mediator.Send(new ImportNode(package, action));
        if (Notifier.ShowIfFailed(import)) return;

        Messenger.Send(new Observer.Created<NodeObserver>(new NodeObserver(import.Value)));

        Notifier.ShowSuccess(
            "Import request complete",
            $"Import of {import.Value.Name} completed successfully @ {DateTime.Now}"
        );
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
        Selected.Clear();
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