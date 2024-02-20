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
public partial class SpecsPageModel : PageViewModel, IRecipient<NodeObserver.Deleted>
{
    [ObservableProperty] private ObservableCollection<NodeObserver> _collections = [];

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

        var observer = new NodeObserver(node) {FocusName = true};
        Collections.Add(observer);
        await Navigator.Navigate(observer);
    }

    public void Receive(NodeObserver.Deleted message)
    {
        if (message.Node.NodeType != NodeType.Collection) return;
        Collections.Remove(message.Node);
    }
}