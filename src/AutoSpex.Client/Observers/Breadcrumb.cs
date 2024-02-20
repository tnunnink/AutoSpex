using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AutoSpex.Client.Observers;

public partial class Breadcrumb : Observer<NodeObserver>
{
    public Breadcrumb(NodeObserver node, CrumbType type) : base(node)
    {
        Type = type;
    }

    [ObservableProperty] private CrumbType _type;

    [ObservableProperty] private bool _inFocus;

    public ObservableCollection<Breadcrumb> Items => new(GetItems());

    public ObservableCollection<Breadcrumb> Children =>
        new(Model.Nodes.Select(n => new Breadcrumb(n, CrumbType.Target)));

    public override Task Navigate()
    {
        return Navigator.Navigate(Model);
    }

    [RelayCommand]
    private void Rename()
    {
        Model.RenameNodeCommand.Execute(null);
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