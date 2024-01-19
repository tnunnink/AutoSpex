using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Components;
using AutoSpex.Client.Messages;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;
using NodeObserver = AutoSpex.Client.Observers.NodeObserver;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class SpecsPageModel : PageViewModel, IRecipient<NodeCreateRequest>, IRecipient<NodeRenamedMessage>
{
    [ObservableProperty] private ObservableCollection<NodeObserver> _collections = [];

    [ObservableProperty] private ObservableCollection<NodeObserver> _selectedNodes = [];

    [ObservableProperty] private NodeObserver? _selectedNode;

    [ObservableProperty] private string _filter = string.Empty;

    public override string Title => "Specs";
    public override string Icon => "Specs";

    protected override async Task Load()
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
        Open(node);
    }

    public void Receive(NodeCreateRequest message) => message.Reply(Mediator.Send(new CreateNode(message.Node)));
    public void Receive(NodeDeleteRequest message) => message.Reply(Mediator.Send(new DeleteNode(message.NodeId)));
    public void Receive(NodeRenamedMessage message)
    {
        //if the node was changed elsewhere (from details page) then we need to update here as well.
        throw new System.NotImplementedException();
    }

    public void Open(NodeObserver node)
    {
        Navigator.NavigateTo<DetailsPageModel>(() => new NodePageModel(node));
    }
}