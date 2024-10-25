using System.Collections.Generic;
using AutoSpex.Client.Resources;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoSpex.Client.Observers;

public partial class SuppressionObserver : Observer<Suppression>
{
    public SuppressionObserver(Suppression model) : base(model)
    {
        Node = GetObserver<NodeObserver>(n => n.Id == Model.NodeId);
    }

    [ObservableProperty] private NodeObserver? _node;

    public string Reason
    {
        get => Model.Reason;
        set => SetProperty(Model.Reason, value, Model, (s, v) => s.Reason = v);
    }

    protected override bool PromptForDeletion => false;

    public override bool Filter(string? filter)
    {
        FilterText = filter;
        return string.IsNullOrEmpty(filter) || Node?.Filter(filter) is true || Reason.Satisfies(filter);
    }

    protected override IEnumerable<MenuActionItem> GenerateContextItems()
    {
        yield return new MenuActionItem
        {
            Header = "Edit Reason",
            Icon = Resource.Find("IconFilledPencil"),
        };

        yield return new MenuActionItem
        {
            Header = "Delete",
            Icon = Resource.Find("IconFilledTrash"),
            Classes = "danger",
            Command = DeleteSelectedCommand,
            Gesture = new KeyGesture(Key.Delete)
        };
    }

    public static implicit operator Suppression(SuppressionObserver observer) => observer.Model;

    public static implicit operator SuppressionObserver(Suppression model) => new(model);
}