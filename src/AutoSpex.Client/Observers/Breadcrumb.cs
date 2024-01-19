using System.Collections.Generic;
using System.Linq;
using AutoSpex.Client.Components;
using AutoSpex.Client.Messages;
using AutoSpex.Client.Pages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Observers;

public partial class Breadcrumb : Observer<NodeObserver>
{
    public Breadcrumb(NodeObserver node, CrumbType type) : base(node)
    {
        Name = Model.Name;
        Type = type;
        HasNodes = Model.Nodes.Count > 0;
    }

    [ObservableProperty] private string _name;

    [ObservableProperty] private CrumbType _type;

    [ObservableProperty] private bool _hasNodes;
    
    [ObservableProperty] private bool _inFocus;

    public IEnumerable<Breadcrumb> Items => GetItems();
    public IEnumerable<Breadcrumb> Children => Model.Nodes.Select(n => n.Breadcrumb);
    
    [RelayCommand]
    private void Navigate()
    {
        Navigator.NavigateTo<DetailsPageModel>(() => new NodePageModel(Model));
    }

    [RelayCommand]
    private void Rename()
    {
        if (string.IsNullOrEmpty(Name) || string.Equals(Name, Model.Name)) return;
        Model.Name = Name;
        Messenger.Send(new NodeRenamedMessage(Model));
    }

    public void ResetName()
    {
        Name = Model.Name;
    }

    private IEnumerable<Breadcrumb> GetItems()
    {
        var items = new List<Breadcrumb> {this};

        var parent = Model.Parent;
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