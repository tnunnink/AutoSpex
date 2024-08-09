using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoSpex.Client.Pages;

public partial class SaveToContainerPageModel : PageViewModel
{
    public override bool KeepAlive => false;
    public ObserverCollection<Node, NodeObserver> Containers { get; private set; } = [];

    [ObservableProperty] private NodeObserver? _selected;

    [ObservableProperty] private string? _filter;

    public override async Task Load()
    {
        var result = await Mediator.Send(new GetContainerNodes());
        if (result.IsFailed) return;
        Containers = new ObserverCollection<Node, NodeObserver>(result.Value.ToList(), x => new NodeObserver(x));
    }

    partial void OnFilterChanged(string? value)
    {
        Containers.Filter(x => x.Name.Satisfies(value));
    }
}