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
    private readonly List<NodeObserver> _container = [];
    public override bool KeepAlive => false;
    public ObservableCollection<NodeObserver> Containers { get; } = [];

    [ObservableProperty] private NodeObserver? _selected;

    [ObservableProperty] private string? _filter;

    public override Task Load()
    {
        var containers = Messenger.Send(new Observer.Find<NodeObserver>(n => n.Type != NodeType.Spec));

        _container.Clear();
        _container.AddRange(containers);

        Containers.Refresh(_container);

        return Task.CompletedTask;
    }

    partial void OnFilterChanged(string? value)
    {
        var filtered = _container.Where(n => n.Name.Satisfies(value));
        Containers.Refresh(filtered);
    }
}