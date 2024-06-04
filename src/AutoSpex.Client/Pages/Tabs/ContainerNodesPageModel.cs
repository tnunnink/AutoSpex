using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AutoSpex.Client.Pages;

public partial class ContainerNodesPageModel : DetailPageModel
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

    partial void OnFilterChanged(string? value)
    {
        var filtered = Node.Descendents.Where(n => n.Filter(value));
        Nodes.Refresh(filtered);
    }
}