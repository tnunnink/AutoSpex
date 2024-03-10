using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Shared;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoSpex.Client.Observers;

public partial class Breadcrumb : Observer<NodeObserver>
{
    public Breadcrumb(NodeObserver node, CrumbType type) : base(node)
    {
        Name = node.Name;
        Type = type;
    }
    
    [ObservableProperty] private string _name;

    [ObservableProperty] private CrumbType _type;

    [ObservableProperty] private bool _inFocus;

    public ObservableCollection<Breadcrumb> Items => new(GetItems());

    public ObservableCollection<Breadcrumb> Children =>
        new(Model.Nodes.Select(n => new Breadcrumb(n, CrumbType.Target)));

    protected override Task Navigate()
    {
        return Navigator.Navigate(Model);
    }

    protected override async Task Rename(string? name)
    {
        await Model.RenameCommand.ExecuteAsync(name);
    }

    private IEnumerable<Breadcrumb> GetItems()
    {
        var items = new List<Breadcrumb> {this};

        var parent = Model.Model.Parent;
        while (parent is not null)
        {
            items.Add(new Breadcrumb(parent, CrumbType.Parent));
            parent = parent.Parent;
        }

        items.Reverse();
        return items;
    }
}

public enum CrumbType
{
    Parent,
    Target
}