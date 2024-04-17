using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentResults;

namespace AutoSpex.Client.Observers;

public partial class Breadcrumb : NamedObserver<Node>
{
    public Breadcrumb(NodeObserver node, CrumbType type) : base(node)
    {
        Type = type;
    }

    public override Guid Id => Model.NodeId;

    public override string Name
    {
        get => Model.Name;
        set => SetProperty(Model.Name, value, Model, (s, v) => s.Name = v);
    }
    
    [ObservableProperty] private CrumbType _type;

    [ObservableProperty] private bool _inFocus;

    public ObservableCollection<Breadcrumb> Items => new(GetItems());
    public ObservableCollection<Breadcrumb> Children => new(GetChildren());

    protected override Task Navigate() => Navigator.Navigate(new NodeObserver(Model));
    protected override Task<Result> RenameModel(string name) => Mediator.Send(new RenameNode(Model));

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

    private IEnumerable<Breadcrumb> GetChildren()
    {
        return Model.Nodes.Select(n => new Breadcrumb(n, CrumbType.Target));
    }
}

public enum CrumbType
{
    Parent,
    Target
}