using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Pages;

public partial class SaveToContainerPageModel : PageViewModel
{
    public override bool KeepAlive => false;
    public ObserverCollection<Node, NodeObserver> Containers { get; } = [];

    [ObservableProperty] private NodeObserver? _selected;

    public override Task Load()
    {
        var message = Messenger.Send(new Observer.Find<NodeObserver>(n => n.Type != NodeType.Spec));
        var containers = message.Responses.Select(n => n.Model).ToList();
        Containers.Bind(containers, x => new NodeObserver(x));
        return Task.CompletedTask;
    }

    protected override void FilterChanged(string? filter)
    {
        Containers.Filter(filter);
    }
}