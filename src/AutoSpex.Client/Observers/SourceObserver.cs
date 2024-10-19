using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoSpex.Client.Pages;
using AutoSpex.Client.Resources;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentResults;

namespace AutoSpex.Client.Observers;

public partial class SourceObserver : Observer<Source>, IRecipient<SourceObserver.Targeted>
{
    /// <inheritdoc/>
    public SourceObserver(Source source) : base(source)
    {
    }

    public override Guid Id => Model.SourceId;
    public override string Icon => nameof(Source);

    public override string Name
    {
        get => Model.Name;
        set => SetProperty(Model.Name, value, Model, (s, v) => s.Name = v);
    }

    public bool IsTarget
    {
        get => Model.IsTarget;
        private set => SetProperty(Model.IsTarget, value, Model, (s, v) => s.IsTarget = v);
    }

    public string TargetName => Model.TargetName;
    public string TargetType => Model.TargetType;
    public string ExportedOn => Model.ExportedOn;
    public string ExportedBy => Model.ExportedBy;
    public string Description => Model.Description;


    #region Commands

    /// <inheritdoc />
    protected override async Task Navigate()
    {
        var result = await Mediator.Send(new LoadSource(Id));
        if (Notifier.ShowIfFailed(result)) return;
        var source = new SourceObserver(result.Value);
        await Navigator.Navigate(source);
    }

    /// <summary>
    /// Sends a target source command to the mediator and notifies observers of the target.
    /// </summary>
    [RelayCommand]
    private async Task Target()
    {
        var result = await Mediator.Send(new TargetSource(Id));
        if (Notifier.ShowIfFailed(result)) return;
        Messenger.Send(new Targeted(this));
    }

    /// <summary>
    /// Command to create and navigate a new run object using this source observer instance.
    /// </summary>
    [RelayCommand]
    private async Task Run()
    {
        //todo need to prompt for node
        var node = Node.NewCollection();

        var result = await Mediator.Send(new NewRun(node.NodeId, Id));
        if (Notifier.ShowIfFailed(result)) return;

        var run = new RunObserver(result.Value);
        await Navigator.Navigate(() => new RunDetailPageModel(run, true));
    }

    #endregion

    #region Messages

    /// <summary>
    /// Update the local <see cref="IsTarget"/> property when the message has been received to keep all instances in sync.
    /// </summary>
    public void Receive(Targeted message)
    {
        if (Id == message.Source.Id)
        {
            IsTarget = true;
            return;
        }

        IsTarget = false;
    }

    /// <summary>
    /// A message that notifies when a source is selected as the target source to be run.
    /// </summary>
    /// <param name="Source">The source instance that was targeted.</param>
    public record Targeted(SourceObserver Source);

    #endregion


    /// <inheritdoc />
    public override bool Filter(string? filter)
    {
        FilterText = filter;
        return Name.Satisfies(filter) || Description.Satisfies(filter) || TargetType.Satisfies(filter);
    }

    /// <inheritdoc />
    protected override Task<Result> UpdateName(string name)
    {
        Model.Name = name;
        return Mediator.Send(new RenameSource(Model));
    }

    /// <inheritdoc />
    protected override Task<Result> DeleteItems(IEnumerable<Observer> observers)
    {
        return Mediator.Send(new DeleteSources(observers.Select(o => o.Id)));
    }

    /// <inheritdoc />
    protected override IEnumerable<MenuActionItem> GenerateMenuItems()
    {
        yield return new MenuActionItem
        {
            Header = "Run",
            Icon = Resource.Find("IconFilledLightning"),
            Classes = "accent",
            Command = RunCommand
        };

        yield return new MenuActionItem
        {
            Header = "Target",
            Icon = Resource.Find("IconLineTarget"),
            Command = TargetCommand
        };

        yield return new MenuActionItem
        {
            Header = "Open",
            Icon = Resource.Find("IconLineLaunch"),
            Command = NavigateCommand
        };

        yield return new MenuActionItem
        {
            Header = "Rename",
            Icon = Resource.Find("IconFilledPencil"),
            Command = RenameCommand,
            Gesture = new KeyGesture(Key.E, KeyModifiers.Control),
        };

        yield return new MenuActionItem
        {
            Header = "Duplicate",
            Icon = Resource.Find("IconFilledClone"),
            Command = DuplicateCommand,
            Gesture = new KeyGesture(Key.D, KeyModifiers.Control),
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

    /// <inheritdoc />
    protected override IEnumerable<MenuActionItem> GenerateContextItems()
    {
        yield return new MenuActionItem
        {
            Header = "Run",
            Icon = Resource.Find("IconFilledLightning"),
            Classes = "accent",
            Command = RunCommand,
            DetermineVisibility = () => HasSingleSelection
        };

        yield return new MenuActionItem
        {
            Header = "Target",
            Icon = Resource.Find("IconLineTarget"),
            Command = TargetCommand,
            DetermineVisibility = () => HasSingleSelection
        };

        yield return new MenuActionItem
        {
            Header = "Open",
            Icon = Resource.Find("IconLineLaunch"),
            Command = NavigateCommand,
            Gesture = new KeyGesture(Key.Enter),
            DetermineVisibility = () => HasSingleSelection
        };

        yield return new MenuActionItem
        {
            Header = "Rename",
            Icon = Resource.Find("IconFilledPencil"),
            Command = RenameCommand,
            Gesture = new KeyGesture(Key.E, KeyModifiers.Control),
            DetermineVisibility = () => HasSingleSelection
        };

        yield return new MenuActionItem
        {
            Header = "Duplicate",
            Icon = Resource.Find("IconFilledClone"),
            Command = DuplicateCommand,
            Gesture = new KeyGesture(Key.D, KeyModifiers.Control),
            DetermineVisibility = () => HasSingleSelection
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

    public static implicit operator Source(SourceObserver observer) => observer.Model;
    public static implicit operator SourceObserver(Source model) => new(model);
}