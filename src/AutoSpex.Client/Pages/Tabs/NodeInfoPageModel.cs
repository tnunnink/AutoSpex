using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class NodeInfoPageModel(NodeObserver node) : PageViewModel
{
    private readonly List<ChangeLogObserver> _changes = [];
    public override string Route => $"{Node.Type}/{Node.Id}/{Title}";
    public override string Title => "Info";
    public NodeObserver Node { get; } = node;
    public ObservableCollection<ChangeLogObserver> Changes { get; } = [];

    [ObservableProperty] private string? _filter;

    [ObservableProperty] private string? _documentation;

    public override async Task Load()
    {
        await LoadChangeLog();
        //todo documentation
    }

    private async Task LoadChangeLog()
    {
        var result = await Mediator.Send(new ListChanges(Node.Id));
        if (result.IsFailed) return;
        
        _changes.Clear();
        _changes.AddRange(result.Value.Select(c => new ChangeLogObserver(c)));
        
        Changes.Refresh(_changes);
    }

    partial void OnFilterChanged(string? value)
    {
        var filtered = _changes.Where(n => n.Filter(value));
        Changes.Refresh(filtered);
    }
}