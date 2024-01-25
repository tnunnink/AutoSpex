using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Messages;
using AutoSpex.Client.Pages.Specs;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;
using NodeObserver = AutoSpex.Client.Observers.NodeObserver;

namespace AutoSpex.Client.Pages.Projects;

[UsedImplicitly]
public partial class SpecsPageModel : PageViewModel, 
    IRecipient<NodeCreateRequest>,
    IRecipient<NodeDeleteRequest>,
    IRecipient<NodeRenameRequest>,
    IRecipient<NodeRenamedMessage>
{
    [ObservableProperty] private ObservableCollection<NodeObserver> _collections = [];

    [ObservableProperty] private ObservableCollection<NodeObserver> _selectedNodes = [];

    [ObservableProperty] private NodeObserver? _selectedNode;

    public override string Title => "Specs";
    public override string Icon => "Specs";

    public override async Task Load()
    {
        var result = await Mediator.Send(new GetNodes());

        if (result.IsSuccess)
        {
            Collections = new ObservableCollection<NodeObserver>(result.Value.Select(n => new NodeObserver(n)));
        }
    }

    [RelayCommand]
    private async Task AddCollection()
    {
        var node = Node.NewCollection();
        var result = await Mediator.Send(new CreateNode(node));

        if (result.IsFailed) return;
        
        Collections.Add(node);
        SelectedNode = node;
        OpenNode(node);
    }
    
    [RelayCommand]
    private void OpenNode(NodeObserver node)
    {
        Navigator.Navigate(() => new NodePageModel(node));
    }

    public void Receive(NodeCreateRequest message) => message.Reply(Mediator.Send(new CreateNode(message.Node)));
    public async void Receive(NodeDeleteRequest message)
    {
        var result = await Mediator.Send(new DeleteNode(message.Node.NodeId));
        message.Reply(result);
        if (result.IsFailed) return;
       
        if (message.Node.NodeType == NodeType.Collection)
        {
            Collections.Remove(message.Node);
        }
    }

    public void Receive(NodeRenameRequest message) => message.Reply(Mediator.Send(new RenameNode(message.Node)));
    public void Receive(NodeRenamedMessage message)
    {
        //if the node was changed elsewhere (from details page) then we need to update here as well.
    }

    
}