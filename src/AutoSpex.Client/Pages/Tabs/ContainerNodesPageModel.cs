using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Pages;

public partial class ContainerNodesPageModel : PageViewModel,
    IRecipient<NodeObserver.Created>,
    IRecipient<NodeObserver.Deleted>
{
    /// <inheritdoc/>
    public ContainerNodesPageModel(NodeObserver node)
    {
        Node = node;
        Nodes.Refresh(node.Descendents);
        Selected.CollectionChanged += OnSelectedNodesChanged;
    }

    private void OnSelectedNodesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        DeleteNodesCommand.NotifyCanExecuteChanged();
        CopyNodesCommand.NotifyCanExecuteChanged();
    }

    public override string Route => $"{Node.Type}/{Node.Id}/{Title}";
    public override string Title => $"{Node.Feature}s";
    public NodeObserver Node { get; }
    public ObservableCollection<NodeObserver> Nodes { get; } = [];
    public ObservableCollection<NodeObserver> Selected { get; } = [];

    [ObservableProperty] private string? _filter;

    public string NoItemsHeader => $"No {Title} defined.";

    [RelayCommand]
    private Task AddNode()
    {
        throw new NotImplementedException();
    }

    [RelayCommand(CanExecute = nameof(NodesSelected))]
    private Task DeleteNodes()
    {
        throw new NotImplementedException();
    }

    [RelayCommand(CanExecute = nameof(NodesSelected))]
    private Task CopyNodes()
    {
        throw new NotImplementedException();
    }

    private bool NodesSelected() => Selected.Count > 0;

    public void Receive(Observer<Node>.Created message)
    {
        if (message.Observer is not NodeObserver node || node.ParentId != Node.Id) return;
        UpdateNodes(Filter);
    }

    public void Receive(Observer<Node>.Deleted message)
    {
        if (message.Observer is not NodeObserver node || node.ParentId != Node.Id) return;
        UpdateNodes(Filter);
    }

    partial void OnFilterChanged(string? value)
    {
        UpdateNodes(value);
    }

    private void UpdateNodes(string? filter)
    {
        var filtered = Node.Descendents.Where(n => n.Filter(filter));
        Nodes.Refresh(filtered);
    }
}