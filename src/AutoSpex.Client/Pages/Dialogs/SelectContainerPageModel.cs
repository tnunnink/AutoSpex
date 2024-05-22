using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoSpex.Client.Pages;

public partial class SelectContainerPageModel(NodeType type) : PageViewModel
{
    public ObservableCollection<NodeObserver> Containers { get; } = [];
    
    [ObservableProperty] private NodeObserver? _selected;
    
    [ObservableProperty] private string? _filter;
    
    public override async Task Load()
    {
        var result = await Mediator.Send(new GetContainerNodes(type));
        if (result.IsFailed) return;
        
        Containers.Clear();
        Containers.AddRange(result.Value.Select(n => new NodeObserver(n)));
    }
}