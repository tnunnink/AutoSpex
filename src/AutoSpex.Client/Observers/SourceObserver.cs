using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoSpex.Client.Resources;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentResults;
using L5Sharp.Core;

namespace AutoSpex.Client.Observers;

public partial class SourceObserver : Observer<Source>,
    IRecipient<SourceObserver.Targeted>,
    IRecipient<Observer.Get<SourceObserver>>
{
    /// <inheritdoc/>
    public SourceObserver(Source source) : base(source)
    {
    }

    public override Guid Id => Model.SourceId;
    public override string Icon => nameof(Source);
    public string ExportedOn => Model.ExportedOn;

    public override string Name
    {
        get => Model.Name;
        set => SetProperty(Model.Name, value, Model, (s, v) => s.Name = v);
    }

    public override string? Description
    {
        get => Model.Description;
        set => SetProperty(Model.Description, value, Model, (s, v) => s.Description = v);
    }

    public bool IsTarget
    {
        get => Model.IsTarget;
        private set => SetProperty(Model.IsTarget, value, Model, (s, v) => s.IsTarget = v);
    }

    #region Commands

    /// <summary>
    /// Sends a target source command to the mediator and notifies observers of the target.
    /// </summary>
    [RelayCommand]
    private async Task Target()
    {
        var result = await Mediator.Send(new TargetSource(Id));
        if (Notifier.ShowIfFailed(result)) return;
        Messenger.Send(new Targeted(result.Value));
    }

    [RelayCommand]
    private async Task UpdateContent()
    {
        var path = await Shell.StorageProvider.SelectSourceFile();
        if (path is null) return;

        try
        {
            var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            var content = await L5X.LoadAsync(path.LocalPath, cancellation.Token);
            Model.Update(content);
            var result = await Mediator.Send(new UpdateSource(Model), cancellation.Token);
            if (Notifier.ShowIfFailed(result)) return;
            Notifier.ShowSuccess("Source content updated", $"Successfully updated content for {Name} @ {DateTime.Now}");
        }
        catch (Exception e)
        {
            Notifier.ShowError("Load Failure", $"Failed to load the selected source file with error {e.Message}.");
        }
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
    /// Handle the get message by replying with this source instnace if it satisfies the provided predicate.
    /// </summary>
    public void Receive(Get<SourceObserver> message)
    {
        if (message.HasReceivedResponse) return;

        if (message.Predicate(this))
        {
            message.Reply(this);
        }
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
        return Name.Satisfies(filter) || Description.Satisfies(filter);
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
        return Mediator.Send(new DeleteSources(observers.Cast<SourceObserver>().Select(o => o.Model)));
    }

    /// <inheritdoc />
    protected override IEnumerable<MenuActionItem> GenerateMenuItems()
    {
        yield return new MenuActionItem
        {
            Header = "Target",
            Icon = Resource.Find("IconLineTarget"),
            Command = TargetCommand
        };

        yield return new MenuActionItem
        {
            Header = "Update",
            Icon = Resource.Find("IconLineUpload"),
            Command = UpdateContentCommand,
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
            Header = "Target",
            Icon = Resource.Find("IconLineTarget"),
            Command = TargetCommand,
            DetermineVisibility = () => HasSingleSelection
        };

        yield return new MenuActionItem
        {
            Header = "Update",
            Icon = Resource.Find("IconLineUpload"),
            Command = UpdateContentCommand,
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