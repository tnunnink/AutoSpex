using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class SpecsPageModel : PageViewModel, IRecipient<NodeObserver.Deleted>
{
    public ObservableCollection<NodeObserver> Collections { get; } = [];
    public ObservableCollection<NodeObserver> SelectedNodes { get; } = [];

    public override string Title => "Specs";
    public override string Icon => "Specs";

    public override async Task Load()
    {
        var result = await Mediator.Send(new GetCollections());
        if (result.IsFailed) return;
        UpdateCollections(result.Value);
    }

    [RelayCommand]
    private async Task AddCollection()
    {
        var node = Node.NewCollection();
        var result = await Mediator.Send(new CreateNode(node));
        if (result.IsFailed) return;

        var observer = new NodeObserver(node);
        Collections.Add(observer);
        await Navigator.Navigate(observer);
    }

    [RelayCommand]
    private async Task DeleteNodes()
    {
        if (SelectedNodes.Count == 0) return;

        if (SelectedNodes.Count == 1)
        {
            var node = SelectedNodes[0];
            await node.DeleteCommand.ExecuteAsync(null);
            return;
        }
        
        //todo prompt for all x amount of nodes
        
        var result = await Mediator.Send(new DeleteNodes(SelectedNodes.Select(n => n.Id)));
        if (result.IsFailed) return;

        var removed = SelectedNodes.ToList();
        foreach (var node in removed)
            Messenger.Send(new NodeObserver.Deleted(node));
        
        SelectedNodes.Clear();
    }

    public void Receive(Observer<Node>.Deleted message)
    {
        if (message.Observer is not NodeObserver observer) return;
        if (observer.NodeType != NodeType.Collection) return;
        Collections.Remove(observer);
    }

    private void UpdateCollections(IEnumerable<Node> nodes)
    {
        Collections.Clear();

        foreach (var node in nodes)
        {
            Collections.Add(new NodeObserver(node));
        }
    }
}