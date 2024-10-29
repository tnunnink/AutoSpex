using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class SelectSpecPageModel : PageViewModel
{
    public string ButtonText { get; init; } = "Add";
    public override bool KeepAlive => false;
    public ObserverCollection<Node, NodeObserver> Nodes { get; } = [];

    [ObservableProperty] private NodeObserver? _selected;

    public override async Task Load()
    {
        var result = await Mediator.Send(new FindNodes(n => n.Type == NodeType.Spec));
        Nodes.BindReadOnly(result.Select(n => new NodeObserver(n)).ToList());
        RegisterDisposable(Nodes);
    }

    protected override void FilterChanged(string? filter)
    {
        Nodes.Filter(filter);
    }
}