using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoSpex.Client.Pages;

public partial class SelectContainerPageModel : PageViewModel
{
    private readonly List<NodeObserver> _containers = [];
    public ObservableCollection<NodeObserver> Containers { get; } = [];
    
    [ObservableProperty] private NodeObserver? _selected;
    
    [ObservableProperty] private string? _filter;
    
    public override async Task Load()
    {
        var result = await Mediator.Send(new GetContainerNodes());
        if (result.IsFailed) return;
        _containers.Clear();
        _containers.AddRange(result.Value.Select(n => new NodeObserver(n)));
        UpdateContainers();
    }

    partial void OnFilterChanged(string? value)
    {
        UpdateContainers(value);
    }

    private void UpdateContainers(string? filter = default)
    {
        var filtered = _containers.Where(n => n.Filter(filter));
        Containers.Refresh(filtered);
    }
}