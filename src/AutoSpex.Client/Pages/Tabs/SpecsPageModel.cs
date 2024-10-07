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

public partial class SpecsPageModel(NodeObserver node) : PageViewModel("Specs"),
    IRecipient<Observer.Created>,
    IRecipient<Observer.Deleted>,
    IRecipient<Observer.GetSelected>,
    IRecipient<NodeObserver.Moved>
{
    public override string Route => $"{node.Type}/{node.Id}/{Title}";
    public ObserverCollection<Node, NodeObserver> Specs { get; private set; } = [];
    public ObservableCollection<NodeObserver> Selected { get; } = [];

    [ObservableProperty] private string? _filter;

    /// <inheritdoc />
    public override async Task Load()
    {
        var result = await Mediator.Send(new GetNode(node.Id));
        if (Notifier.ShowIfFailed(result, $"Failed to retrieve data for {node.Name}")) return;

        var nodes = result.Value.Descendants(NodeType.Spec).ToList();
        Specs = new ObserverCollection<Node, NodeObserver>(nodes, n => new NodeObserver(n));
    }

    /// <summary>
    /// A command to add a new spec node to this containing node.
    /// </summary>
    [RelayCommand]
    private async Task AddSpec()
    {
        //Adds to the current parent node.
        var spec = node.Model.AddSpec();

        var result = await Mediator.Send(new CreateNode(spec));
        if (Notifier.ShowIfFailed(result)) return;

        var observer = new NodeObserver(spec) { IsNew = true };
        Specs.Add(observer);
        Messenger.Send(new Observer.Created(observer));
    }

    /// <summary>
    /// Handle reception of a spec node being created by adding it to the local spec collection if the node is a
    /// descendant of the current node.
    /// </summary>
    public void Receive(Observer.Created message)
    {
        if (message.Observer is not NodeObserver observer) return;
        if (!observer.Model.IsDescendantOf(node)) return;
        if (observer.Type != NodeType.Spec) return;
        if (Specs.Contains(observer)) return;
        Specs.Add(new NodeObserver(observer));
    }

    /// <summary>
    /// Handle deletions of child node instances. This should be a flat list of descendants, so we can just iterate
    /// to find the matching node.
    /// </summary>
    public void Receive(Observer.Deleted message)
    {
        if (message.Observer is not NodeObserver observer) return;
        Specs.RemoveAny(s => s.Id == observer.Id);
    }

    /// <summary>
    /// Handle reception of the get select nodes for this view. Only return if the selected nodes contains the instance
    /// that is requesting the selection.
    /// </summary>
    public void Receive(Observer.GetSelected message)
    {
        if (message.Observer is not NodeObserver) return;
        if (!Selected.Any(s => s.Is(message.Observer))) return;

        foreach (var observer in Selected)
            message.Reply(observer);
    }

    /// <summary>
    /// Handle the reception of the node moved message my adding or removing the name based on it's place in the
    /// heirarchy. 
    /// </summary>
    public void Receive(NodeObserver.Moved message)
    {
        if (message.Node.Model.IsDescendantOf(node) && !Specs.Contains(message.Node))
        {
            Specs.Add(message.Node);
        }

        if (!message.Node.Model.IsDescendantOf(node) && Specs.Contains(message.Node))
        {
            Specs.Remove(message.Node);
        }
    }

    /// <summary>
    /// When the filter text changes call the observer collection filter method to update the filter text and perform
    /// the filtering of the underlying collection
    /// </summary>
    partial void OnFilterChanged(string? value)
    {
        Specs.Filter(n => n.FilterSingle(value) || n.Model.Path.Satisfies(value));
    }
}