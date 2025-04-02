using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoSpex.Client.Pages;

public partial class SelectContainerPageModel : PageViewModel
{
    public string Header { get; init; } = "Select container";
    public string ButtonText { get; init; } = "Next";
    public override bool KeepAlive => false;
    public ObserverCollection<Node, NodeObserver> Containers { get; } = [];

    [ObservableProperty] private NodeObserver? _selected;

    public override async Task Load()
    {
        var result = await Mediator.Send(new FindNodes(n => n.Type != NodeType.Spec));
        Containers.BindReadOnly(result.Select(n => new NodeObserver(n)).ToList());
        RegisterDisposable(Containers);
    }

    protected override void FilterChanged(string? filter)
    {
        Containers.Filter(filter);
    }
}