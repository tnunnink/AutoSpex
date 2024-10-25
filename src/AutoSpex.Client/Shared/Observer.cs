using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoSpex.Client.Services;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using FluentResults;

namespace AutoSpex.Client.Shared;

/// <summary>
/// A generic view model class that represents a entity or item that is has shared common functionality.
/// This class is intended to wrap model classes and add UI related functionality for the application.
/// </summary>
public abstract partial class Observer : TrackableViewModel, IEquatable<Observer>, IRecipient<Observer.Renamed>
{
    /// <summary>
    /// Creates a new <see cref="Observer{TModel}"/> with the provided model object.
    /// </summary>
    protected Observer()
    {
        IsActive = true;
    }

    /// <summary>
    /// A <see cref="Guid"/> that uniquely identifies this observer. This should be the same for each instance wrapping
    /// the same underlying model. By default, this just creates a new <see cref="Guid"/> but deriving classes will
    /// implement to point to the correct model id. 
    /// </summary>
    public virtual Guid Id { get; } = Guid.NewGuid();

    /// <summary>
    /// The name of the observer. Most observers will have a name but if not then this can be left alone. Deriving classes
    /// can implement how this property is set and retrieved.
    /// </summary>
    public virtual string Name { get; set; } = string.Empty;

    /// <summary>
    /// The name of the icon that commonly represents this observer. This can be used in data templates to bind to
    /// and along with our K
    /// </summary>
    public virtual string Icon => string.Empty;

    /// <summary>
    /// Indicates that this observer should be visible in the UI. This property is set via the <see cref="Filter"/>
    /// method based on the input <see cref="FilterText"/>
    /// </summary>
    [ObservableProperty] private bool _isVisible = true;

    /// <summary>
    /// Indicates that this observer is selected in the UI. Controls can bind to this to control the state.
    /// </summary>
    [ObservableProperty] private bool _isSelected;

    /// <summary>
    /// Indicates that this observer is expanded in the UI (assuming it's a tree view item). this can be bound to in
    /// order to control the expanded state of the tree.
    /// </summary>
    [ObservableProperty] private bool _isExpanded;

    /// <summary>
    /// Indicates that this observer is "checked" in the UI. This can be bound to from a checkbox control or similar
    /// to control the selection of items.
    /// </summary>
    [ObservableProperty] private bool _isChecked;

    /// <summary>
    /// Indicates that this is a "new" observer that has not been persisted. Deriving classes can use this to drive
    /// the state of the UI as needed.
    /// </summary>
    [ObservableProperty] private bool _isNew;

    /// <summary>
    /// The current filter text being applied to the observer. This is here because many observers we want trigger
    /// filtering based on some entered text/keyword. When changed, the <see cref="Filter"/> method will be called and
    /// deriving classes can define how the observer is filtered. This property can also be bound to highlight the
    /// portion of the item template that matches the input filter.
    /// </summary>
    [ObservableProperty] private string? _filterText;

    /// <summary>
    /// The collection of <see cref="MenuActionItem"/> objects configured for the observer which are shown in the
    /// context menu when an observer is right-clicked. These are configured since we want to control some of the options
    /// dynamically.
    /// </summary>
    public IEnumerable<MenuActionItem> MenuItems => GenerateMenuItems();

    /// <summary>
    /// The collection of <see cref="MenuActionItem"/> objects configured for the observer which are shown in the
    /// context menu when an observer is right-clicked. These are configured since we want to control some of the options
    /// dynamically.
    /// </summary>
    public IEnumerable<MenuActionItem> ContextItems => GenerateContextItems();

    /// <summary>
    /// Gets a value indicating whether this observer belongs to a items collection where only a single item is selected.
    /// We need to know this for context menu items, as the options would change based on how many items the user is
    /// selecting.
    /// </summary>
    /// <remarks>
    /// To accomplish this, we are using the <see cref="SelectedItems"/> collection with is retrieved by sending a request
    /// message for selected observers that are siblings (contained in the same list or tree) as this observer.
    /// </remarks>
    protected bool HasSingleSelection => SelectedItems.Count() == 1;

    /// <summary>
    /// A collection of other <see cref="Observer"/> instances that are selected and part of the same containing
    /// UI control (list/tree/etc.). We want to know if the user has selected multiple observers, so we can drive the
    /// display of context items and what actions they may take (delete selected, move selected, etc.). This uses the
    /// <see cref="GetSelected"/> message which should be handled in each page the observers are displayed within an
    /// items control (and assuming this functionality is needed).
    /// </summary>
    protected IEnumerable<Observer> SelectedItems => Messenger.Send(new GetSelected(this)).Responses;

    /// <summary>
    /// Indicates whether this observer should by default prompt the user when it or it's selected siblings are being
    /// deleted from the application.
    /// </summary>
    protected virtual bool PromptForDeletion => true;

    /// <summary>
    /// A function to apply filtering to this <see cref="Observer"/> object given an input filter text. returns
    /// <c>true</c> if the filter input is null or empty or the <see cref="Name"/> contains the filter text, which is the
    /// default condition for most items. Deriving observers can override to further define how to filter each object.
    /// This method will also set the <see cref="IsVisible"/> property, so items controls can bind to this property and
    /// filtering is automatically taken care of. This would reduce the number of places we need to rewrite filter
    /// functionality.
    /// </summary>
    /// <param name="filter">The input filter text.</param>
    /// <returns><c>true</c> if this object passes the filter provided, otherwise, <c>false</c>.</returns>
    public virtual bool Filter(string? filter)
    {
        FilterText = filter;
        return string.IsNullOrEmpty(filter) || Name.Satisfies(filter);
    }

    /// <summary>
    /// Attempts to find a parent observer instance of the specified type using the current observer <see cref="Id"/>.
    /// This would allow observer or page to request its parent from another observer/page assuming it is in memory.
    /// This allows for loose coupling of parent child relationships and the ability to get a references to a specific
    /// object from across pages in the application.
    /// </summary>
    /// <typeparam name="TObserver">The observer type to find.</typeparam>
    /// <returns>
    /// The observer instance of the specified type that contains (depending on how handler logic) the provided id.
    /// </returns>
    /// <remarks>
    /// If the instance is not in memory, and therefore the message has no response, this method returns
    /// null, and it is up to the caller to handle this situation.
    /// </remarks>
    protected TObserver? GetObserver<TObserver>(Func<TObserver, bool> predicate) where TObserver : Observer
    {
        var request = new Get<TObserver>(predicate);
        Messenger.Send(request);
        return request.HasReceivedResponse ? request.Response : default;
    }

    #region Commands

    /// <inheritdoc />
    protected override Task Navigate()
    {
        return Navigator.Navigate(this);
    }

    /// <summary>
    /// A command to issue deletion of this <see cref="Observer{TModel}"/> object from the database.
    /// </summary>
    /// <returns>The <see cref="Task"/> representing the async function to perform.</returns>
    /// <remarks>
    /// Deriving classes are also expected to send the <see cref="Deleted"/> message to notify other observers
    /// or pages that the object has been deleted, so they can respond accordingly.
    /// </remarks>
    [RelayCommand]
    private async Task Delete()
    {
        if (PromptForDeletion)
        {
            var delete = await Prompter.PromptDelete(Name);
            if (delete is not true) return;
        }

        var result = await DeleteItems([this]);
        if (Notifier.ShowIfFailed(result,
                $"Failed to delete {Name} due to {result.Reasons.FirstOrDefault()?.Message}")) return;

        Messenger.Send(new Deleted(this));
    }

    /// <summary>
    /// Deletes the items that are return by the <see cref="SelectedItems"/> collection for this observer.
    /// Therefore, this requires that some page is responding to the <see cref="GetSelected"/> message for this observer type.
    /// This command will also prompt the user before deleting the selected items.
    /// This command makes use of <see cref="DeleteItems"/> which deriving classes implement to update the database.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [RelayCommand]
    private async Task DeleteSelected()
    {
        var selected = SelectedItems.ToList();

        if (PromptForDeletion)
        {
            var message = selected.Count == 1 ? Name : $"{selected.Count.ToString()} selected items";
            var delete = await Prompter.PromptDelete(message);
            if (delete is not true) return;
        }

        var result = await DeleteItems(selected);
        if (result.IsFailed) return;

        foreach (var deleted in selected)
            Messenger.Send(new Deleted(deleted));
    }

    /// <summary>
    /// A command that updates the name of the underlying item model and sends a command to update the persisted value.
    /// </summary>
    /// <param name="name">The new name of the item.</param>
    [RelayCommand]
    protected virtual async Task Rename(string? name)
    {
        //If empty prompt the user for a new name.
        if (string.IsNullOrEmpty(name))
        {
            name = await Prompter.PromptRename(this);
        }

        //If still null or empty return.
        if (string.IsNullOrEmpty(name)) return;

        var result = await UpdateName(name);
        if (Notifier.ShowIfFailed(result, $"Failed to rename item: {result.Reasons}")) return;

        Name = name;
        IsNew = false;

        Messenger.Send(new Renamed(this));
    }

    /// <summary>
    /// A command to duplicate the <see cref="Observer"/> object.
    /// The default implementation does nothing and not all observers may need this, but it will be supported by more than one so
    /// this is to consolidate the code. 
    /// </summary>
    /// <returns>The <see cref="Task"/> representing the async function to perform.</returns>
    [RelayCommand]
    protected virtual Task Duplicate() => Task.FromResult(Messenger.Send(new MakeCopy(this)));

    /// <summary>
    /// A command to move and observer object in the list of observers to a different location.
    /// This could be a different index in a list or different leaf in a tree.
    /// The default implementation does nothing. Deriving observers requiring the feature must implement.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanMove))]
    protected virtual Task Move(object? source) => Task.CompletedTask;

    /// <summary>
    /// Determines if the move command can be executed. This can help determine if drag/drop items are accepted by
    /// the observer or not.
    /// </summary>
    /// <returns><c>true</c> if the object can be moved here; otherwise, <c>false</c></returns>
    protected virtual bool CanMove(object? source) => false;

    /// <summary>
    /// A command to copy the <see cref="Observer"/> object to the clipboard so that the user can paste it somewhere
    /// else that handles or accepts a paste command. By default, this command sets the name of the observer to the clipboard.
    /// NOTE: At this time Avalonia Clipboard does not accept getting data objects as they are set, so I can't use
    /// the actual object instance. This is why we are using serialization, which most of our engine objects support anyway.
    /// Deriving observers should override this implementation as needed.
    /// </summary>
    [RelayCommand]
    protected virtual async Task Copy()
    {
        var clipboard = Shell.Clipboard;
        if (clipboard is null) return;
        await clipboard.SetTextAsync(Name);
    }

    /// <summary>
    /// A helper method that allows derived classes to get data from the clipboard. This assumes a collection of
    /// objects of the specified type have been serialized to the clipboard as JSON. This is how we will handle copying
    /// objects since the current Avalonia CLipboard does not support getting data objects in memory.
    /// </summary>
    /// <typeparam name="TData">The model type that was set on the clipboard.</typeparam>
    protected async Task<List<TData>> GetClipboardObservers<TData>()
    {
        try
        {
            var clipboard = Shell.Clipboard;
            if (clipboard is null) return [];

            var json = await clipboard.GetTextAsync();
            if (json is null) return [];

            var observers = JsonSerializer.Deserialize<List<TData>>(json);
            return observers ?? [];
        }
        catch (Exception e)
        {
            Notifier.ShowError("Unable to parse data from clipboard.", e.Message);
        }

        return [];
    }

    #endregion

    #region Handlers

    /// <summary>
    /// Handles the observer renamed message by updating this object's name if it is not the sender of the
    /// message. This will keep the observer instances in sync.
    /// </summary>
    public virtual void Receive(Renamed message)
    {
        if (Id != message.Observer.Id) return;

        if (Name != message.Observer.Name)
        {
            Name = message.Observer.Name;
            Messenger.Send(new Renamed(this));
        }

        OnPropertyChanged(nameof(Name));
    }

    #endregion

    #region Messages

    /// <summary>
    /// A message that indicates a new observer was created. This can be sent from various places and handled in pages
    /// that need to respond/update to reflect the newly created observer.
    /// </summary>
    /// <param name="Observer">The observer that was created.</param>
    public record Created<TObserver>(TObserver Observer) where TObserver : Observer;

    /// <summary>
    /// A messages that indicates an existing observer was deleted. This can be used to notify other pages that need
    /// to respond/update the UI to reflect the no longer existing observer.
    /// </summary>
    /// <param name="Observer">The observer that was deleted.</param>
    public record Deleted(Observer Observer);

    /// <summary>
    /// A message to be sent when the observer name changes so that other instances can respond and update their
    /// local value and in turn refresh the UI.
    /// </summary>
    public record Renamed(Observer Observer);

    /// <summary>
    /// A message that indicates the observer needs to be duplicated or copied.
    /// This can be handled by parent observers or pages to refresh the UI to show the newly duplicated instance.
    /// This is by default what the <see cref="Shared.Observer.DuplicateCommand"/> sends.
    /// </summary>
    /// <param name="Observer">The observer instance that needs to be duplicated.</param>
    public record MakeCopy(Observer Observer);

    /// <summary>
    /// A request to get an in memory instance of an observer that satisfies the provided condition. This will return
    /// a single observer instance, and if multiple are in memory, should return the first instance that recieves the
    /// message.
    /// </summary>
    /// <param name="predicate">The predicate the observer must satisfy.</param>
    /// <typeparam name="TObserver">The type of observer the request is representing.</typeparam>
    public class Get<TObserver>(Func<TObserver, bool> predicate) : RequestMessage<TObserver> where TObserver : Observer
    {
        public Func<TObserver, bool> Predicate { get; } = predicate;
    }

    /// <summary>
    /// A request message that will return all selected <see cref="Observer"/> instances to the requesting object.
    /// These message will have to be handled by containing pages since the "same" (equivalent) observer could in more
    /// than on active control at a time. The only way to delineate is using referential equality, meaning we need to supply
    /// the reference object requesting selected siblings. Only items collection containing this instance should respond.
    /// </summary>
    public class GetSelected(Observer observer) : CollectionRequestMessage<Observer>
    {
        public Observer Observer { get; } = observer;
    }
    
    /// <summary>
    /// A request to retrieve all in memory instances of and observer that satisfy the provided predicate.
    /// </summary>
    /// <param name="predicate">The predicate the observer must satisfy.</param>
    /// <typeparam name="TObserver">The type of observer the request is representing.</typeparam>
    public class Find<TObserver>(Func<TObserver, bool> predicate)
        : AsyncCollectionRequestMessage<TObserver> where TObserver : Observer
    {
        public Func<TObserver, bool> Predicate { get; } = predicate;
    }

    #endregion

    #region Equality

    /// <summary>
    /// Determines whether the current instance is referentially equal or the same as the provided instance.
    /// </summary>
    /// <param name="other">The <see cref="Observer"/> object to compare with the current instance.</param>
    /// <returns><c>true</c> if the current instance is referentially equal to the provided instance; otherwise, <c>false</c>.</returns>
    public bool Is(Observer other) => ReferenceEquals(this, other);

    /// <inheritdoc />
    public bool Equals(Observer? other)
    {
        if (ReferenceEquals(this, other)) return true;
        return other is not null && other.Id == Id;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj)) return true;
        return obj is Observer other && other.Id == Id;
    }

    /// <inheritdoc />
    public override int GetHashCode() => Id.GetHashCode();

    /// <inheritdoc />
    public override string ToString() => Name;

    public static bool operator ==(Observer first, Observer second) => Equals(first, second);
    public static bool operator !=(Observer first, Observer second) => !Equals(first, second);

    #endregion

    /// <summary>
    /// A method to update the underlying model name property and send any necessary command to update the name in the
    /// database.
    /// </summary>
    /// <param name="name">The new name value.</param>
    /// <returns>A result indicating the success or failure of the update.</returns>
    protected virtual Task<Result> UpdateName(string name) => Task.FromResult(Result.Ok());

    /// <summary>
    /// The task to delete the provided items from the database if required by this observer. This is called by
    /// the <see cref="DeleteCommand"/> and <see cref="DeleteSelectedCommand"/>. By default, this simply returns an OK
    /// result to allow deletion. Deriving classes implement this method to forward call to specific mediator request.
    /// </summary>
    /// <param name="observers">The observer instances to delete. This could be selected items or single item.</param>
    /// <returns>A result indicating the success or failure of the update.</returns>
    protected virtual Task<Result> DeleteItems(IEnumerable<Observer> observers) => Task.FromResult(Result.Ok());

    /// <summary>
    /// Gets the configured collection of <see cref="MenuActionItem"/> to display in the UI.
    /// </summary>
    protected virtual IEnumerable<MenuActionItem> GenerateMenuItems() => [];

    /// <summary>
    /// Gets the configured collection of <see cref="MenuActionItem"/> to display in the UI.
    /// </summary>
    protected virtual IEnumerable<MenuActionItem> GenerateContextItems() => [];
}

/// <summary>
/// <para>
/// A wrapper for our engine models types so that we can expose and decorate the objects with UI related functionality.
/// </para>
/// </summary>
/// <typeparam name="TModel">The model type to wrap.</typeparam>
public abstract class Observer<TModel> : Observer
{
    /// <summary>
    /// Creates a new <see cref="Observer{TModel}"/> with the provided model object.
    /// </summary>
    /// <param name="model">The model instance to wrap.</param>
    /// <typeparam name="TModel">The model type to wrap.</typeparam>
    protected Observer(TModel model)
    {
        Model = model ?? throw new ArgumentNullException(nameof(model));
    }

    /// <summary>
    /// The underlying model object that is being wrapped by the observer.
    /// </summary>
    public TModel Model { get; }
}

public abstract class NullableObserver<TModel>(TModel? model) : Observer
{
    public TModel? Model { get; } = model;
}