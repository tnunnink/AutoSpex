using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoSpex.Client.Observers;

public partial class Breadcrumb : ObservableObject
{
    public Breadcrumb(NodeObserver node, CrumbType type)
    {
        Node = node;
        Name = Node.Name;
        Type = type;
        HasNodes = Node.Nodes.Count > 0;
    }

    [ObservableProperty] private string _name;

    [ObservableProperty] private CrumbType _type;

    [ObservableProperty] private bool _hasNodes;

    public NodeObserver Node { get; }
    public IEnumerable<Breadcrumb> Items => GetItems();
    public IEnumerable<Breadcrumb> Children => Node.Nodes.Select(n => n.Breadcrumb);

    public void AcceptName()
    {
        if (string.IsNullOrEmpty(Name) || string.Equals(Name, Node.Name)) return;
        Node.Name = Name;
    }

    public void ResetName()
    {
        Name = Node.Name;
    }

    private IEnumerable<Breadcrumb> GetItems()
    {
        var items = new List<Breadcrumb> {this};

        var parent = Node.Parent;
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