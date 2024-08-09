using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoSpex.Client.Resources;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentResults;
using Environment = AutoSpex.Engine.Environment;

namespace AutoSpex.Client.Observers;

public partial class EnvironmentObserver : Observer<Environment>,
    IRecipient<EnvironmentObserver.Targeted>,
    IRecipient<Observer.Deleted>,
    IRecipient<Observer.Duplicated>
{
    /// <inheritdoc/>
    public EnvironmentObserver(Environment model) : base(model)
    {
        Sources = new ObserverCollection<Source, SourceObserver>(Model.Sources, s => new SourceObserver(s));
        Track(nameof(Comment));
        Track(Sources);
    }

    public override Guid Id => Model.EnvironmentId;
    public override string Icon => nameof(Environment);

    public override string Name
    {
        get => Model.Name;
        set => SetProperty(Model.Name, value, Model, (s, v) => s.Name = v);
    }

    public string? Comment
    {
        get => Model.Comment;
        set => SetProperty(Model.Comment, value, Model, (s, v) => s.Comment = v);
    }

    public bool IsTarget
    {
        get => Model.IsTarget;
        private set => SetProperty(Model.IsTarget, value, Model, (s, v) => s.IsTarget = v);
    }

    public ObserverCollection<Source, SourceObserver> Sources { get; }

    [RelayCommand]
    private async Task Target()
    {
        var result = await Mediator.Send(new TargetEnvironment(Id));
        if (result.IsFailed) return;
        Messenger.Send(new Targeted(this));
    }

    /// <summary>
    /// Update the local <see cref="IsTarget"/> property when the message has been received to keep all instances in sync.
    /// </summary>
    public void Receive(Targeted message)
    {
        if (Id == message.Environment.Id)
        {
            IsTarget = true;
            return;
        }

        IsTarget = false;
    }

    /// <summary>
    /// Remove the source when requested.
    /// </summary>
    public void Receive(Deleted message)
    {
        if (message.Observer is not SourceObserver source) return;
        Sources.Remove(source);
    }

    /// <summary>
    /// Handles the duplication of a child source observer.
    /// </summary>
    /// <param name="message"></param>
    public void Receive(Duplicated message)
    {
        if (message.Source is not SourceObserver source) return;
        if (message.Duplicate is not SourceObserver duplicate) return;

        if (Sources.Contains(source) && !Sources.Contains(duplicate))
        {
            Sources.Add(duplicate);
        }
    }

    /// <inheritdoc />
    protected override Task<Result> UpdateName(string name)
    {
        Model.Name = name;
        return Mediator.Send(new RenameEnvironment(this));
    }

    /// <inheritdoc />
    protected override Task<Result> DeleteItems(IEnumerable<Observer> observers)
    {
        return Mediator.Send(new DeleteEnvironments(observers.Select(o => o.Id)));
    }

    /// <summary>
    /// A message that notifies when an environment is selected as the target environment to be run.
    /// </summary>
    /// <param name="Environment"></param>
    public record Targeted(EnvironmentObserver Environment);

    /// <inheritdoc />
    protected override IEnumerable<MenuActionItem> GenerateContextItems()
    {
        yield return new MenuActionItem
        {
            Header = "Run",
            Icon = Resource.Find("IconFilledLightning"),
            Classes = "accent"
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

    /// <inheritdoc />
    protected override IEnumerable<MenuActionItem> GenerateMenuItems()
    {
        yield return new MenuActionItem
        {
            Header = "Run",
            Icon = Resource.Find("IconFilledLightning"),
            Classes = "accent"
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
            Gesture = new KeyGesture(Key.E, KeyModifiers.Control)
        };

        yield return new MenuActionItem
        {
            Header = "Duplicate",
            Icon = Resource.Find("IconFilledClone"),
            Command = DuplicateCommand,
            Gesture = new KeyGesture(Key.D, KeyModifiers.Control)
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

    public static implicit operator Environment(EnvironmentObserver observer) => observer.Model;
    public static implicit operator EnvironmentObserver(Environment model) => new(model);
}