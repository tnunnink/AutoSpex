using System.Collections.Generic;
using AutoSpex.Client.Pages;
using AutoSpex.Client.Resources;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using Avalonia.Input;

namespace AutoSpex.Client.Observers;

public class ActionObserver : Observer<Action>
{
    public ActionObserver(Action model) : base(model)
    {
        Node = GetObserver<NodeObserver>(n => n.Id == Model.NodeId);

        Track(nameof(Reason));
        
        if (Model.Config is not null)
        {
            Config = new SpecObserver(Model.Config);
            Track(Config);
        }
    }

    public override string Name => Node?.Name ?? "Unknown";
    protected override bool PromptForDeletion => false;
    public ActionType Type => Model.Type;
    public NodeObserver? Node { get; }
    public SpecObserver? Config { get; }

    public string Reason
    {
        get => Model.Reason;
        set => SetProperty(Model.Reason, value, Model, (s, v) => s.Reason = v);
    }

    /// <inheritdoc />
    /// <remarks>Overriding to open the config page for the rule to allow users to update the rule settings.</remarks>
    protected override async Task Navigate()
    {
        await Navigator.Navigate(() => new ActionConfigPageModel(this));
    }

    /// <inheritdoc />
    public override bool Filter(string? filter)
    {
        FilterText = filter;
        return string.IsNullOrEmpty(filter) || Node?.Filter(filter) is true || Reason.Satisfies(filter);
    }

    protected override IEnumerable<MenuActionItem> GenerateContextItems()
    {
        yield return new MenuActionItem
        {
            Header = "Settings",
            Icon = Resource.Find("IconFilledCog"),
            Command = NavigateCommand
        };

        yield return new MenuActionItem
        {
            Header = $"Open {Node?.Type}",
            Icon = Resource.Find("IconLineLaunch"),
            Command = Node?.NavigateCommand
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

    protected override IEnumerable<MenuActionItem> GenerateMenuItems()
    {
        yield return new MenuActionItem
        {
            Header = "Settings",
            Icon = Resource.Find("IconFilledCog"),
            Command = NavigateCommand
        };

        yield return new MenuActionItem
        {
            Header = $"Open {Node?.Type}",
            Icon = Resource.Find("IconLineLaunch"),
            Command = Node?.NavigateCommand
        };

        yield return new MenuActionItem
        {
            Header = "Delete",
            Icon = Resource.Find("IconFilledTrash"),
            Classes = "danger",
            Command = DeleteCommand,
            Gesture = new KeyGesture(Key.Delete)
        };
    }

    public static implicit operator Action(ActionObserver observer) => observer.Model;
    public static implicit operator ActionObserver(Action model) => new(model);
}