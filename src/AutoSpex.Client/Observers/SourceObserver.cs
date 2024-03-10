using System;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Observers;

public partial class SourceObserver(Source source) : Observer<Source>(source),
    IRecipient<Observer<Source>.Renamed>,
    IRecipient<SourceObserver.Selected>
{
    public override Guid Id => Model.SourceId;

    public string Name
    {
        get => Model.Name;
        set => SetProperty(Model.Name, value, Model, (s, v) => s.Name = v, true);
    }

    public string Description
    {
        get => Model.Description;
        set => SetProperty(Model.Description, value, Model, (s, v) => s.Description = v);
    }

    public bool IsSelected
    {
        get => Model.IsSelected;
        set => SetProperty(Model.IsSelected, value, Model, (s, v) => s.IsSelected = v);
    }

    public string TargetName => Model.TargetName;
    public string TargetType => Model.TargetType;
    public string TargetTypeFormatted => $"[{TargetType}]";
    public string ExportedBy => Model.ExportedBy;
    public DateTime ExportedOn => Model.ExportedOn;

    /// <inheritdoc />
    protected override async Task Delete()
    {
        var delete = await Prompter.PromptDelete(Name);
        if (delete is not true) return;

        var result = await Mediator.Send(new DeleteSource(Id));
        if (result.IsFailed) return;

        Messenger.Send(new Deleted(this));
        Messenger.UnregisterAll(this);
    }

    /// <inheritdoc />
    protected override async Task Rename(string? name)
    {
        if (string.IsNullOrEmpty(name) || string.Equals(name, Name) || name.Length > 100) return;

        var previous = Name;
        Name = name;

        var result = await Mediator.Send(new RenameSource(this));

        if (result.IsFailed)
        {
            Name = previous;
            return;
        }

        Messenger.Send(new Renamed(this));
    }

    [RelayCommand]
    private void Select()
    {
        IsSelected = true;
        Messenger.Send(new Selected(Id));
    }

    /// <summary>
    /// Updates the local <see cref="Name"/> property if the observer that changed represents the same observer as
    /// this object has not yet updated it's name. This will allow all observer instance to sync their name property
    /// once one is changed
    /// </summary>
    /// <param name="message">The message received representing the observer name change.</param>
    public void Receive(Renamed message)
    {
        if (message.Observer is not SourceObserver source) return;
        if (ReferenceEquals(this, source)) return;
        if (source.Id != Id) return;
        if (Name == source.Name) return;
        Name = source.Name;
        Messenger.Send(new Renamed(this));
    }

    public void Receive(Selected message)
    {
        if (Id == message.SourceId) return;
        IsSelected = false;
    }

    public static implicit operator Source(SourceObserver observer) => observer.Model;
    public static implicit operator SourceObserver(Source source) => new(source);

    public record Selected(Guid SourceId);
}