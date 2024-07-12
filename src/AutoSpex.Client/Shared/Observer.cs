using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Indicates that this observer is being "edited" meaning the name entry field is being available. This is used
    /// in conjunction with the Rename functionality to show the entry for renaming an observer, but deriving classes
    /// can also use this for other purposes.
    /// </summary>
    [ObservableProperty] private bool _isEditing;

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
    /// Gets a value indicating whether this observer is solely selected.
    /// </summary>
    /// <remarks>
    /// The observer is considered solely selected if there is only one item selected in the
    /// <see cref="SelectedItems"/> collection and the <see cref="IsSelected"/> property is true.
    /// </remarks>
    public bool IsSolelySelected => SelectedItems.Count() == 1 && IsSelected;

    /// <summary>
    /// A collection of other <see cref="Observer"/> instances that are selected and part of the same containing
    /// UI control (list/tree/etc.). We want to know if the user has selected multiple observers, so we can drive the
    /// display of menu items and what actions they may take (delete selected, move selected, etc.). This uses the
    /// <see cref="GetSelected"/> message which should be handled in each page the observers are displayed within an
    /// items control.
    /// </summary>
    public IEnumerable<Observer> SelectedItems => Messenger.Send(new GetSelected(this)).Responses;

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
    /// A function to apply filtering to this <see cref="Observer"/> object given an input filter text. returns
    /// <c>true</c> if the filter input is null or empty or the <see cref="Name"/> contains the filter text, which is the
    /// default condition for most items. Deriving observer can override to further define how to filter each object.
    /// This method will also set the <see cref="IsVisible"/> property, so items controls can bind to this property and
    /// filtering is automatically taken care of. This would reduce the number of places we need to rewrite filter
    /// functionality.
    /// </summary>
    /// <param name="filter">The input filter text.</param>
    /// <returns><c>true</c> if this object passes the filter provided, otherwise, <c>false</c>.</returns>
    public virtual bool Filter(string? filter)
    {
        FilterText = filter;
        IsVisible = string.IsNullOrEmpty(filter) || Name.Satisfies(filter);
        return IsVisible;
    }

    /// <summary>
    /// Attempts to find an observer instance of the specified type using the current instance's <see cref="Id"/>.
    /// This would allow observer or page to request an instance from another place assuming it is in memory.
    /// This allows of loose coupling of parent child relationships and the ability to get a references to a specific
    /// object from across pages in the application.
    /// </summary>
    /// <typeparam name="TObserver">The observer type to find.</typeparam>
    /// <returns>
    /// The observer instance of the specified type that has or contains (depending on how handler logic)
    /// the provided id.
    /// </returns>
    /// <remarks>
    /// If the instance is not in memory, and therefore the message has no response, this method returns
    /// null, and it is up to the caller to handle this situation.
    /// </remarks>
    protected TObserver? FindInstance<TObserver>() where TObserver : Observer
    {
        var request = new Request<TObserver>(Id);
        Messenger.Send(request);
        return request.HasReceivedResponse ? request.Response : default;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <typeparam name="TObserver"></typeparam>
    /// <returns></returns>
    protected TObserver? FindInstance<TObserver>(Guid id) where TObserver : Observer
    {
        var request = new Request<TObserver>(id);
        Messenger.Send(request);
        return request.HasReceivedResponse ? request.Response : default;
    }

    #region Commands

    /// <summary>
    /// A command to issue deletion of this <see cref="Observer{TModel}"/> object from the database.
    /// </summary>
    /// <returns>The <see cref="Task"/> representing the async function to perform.</returns>
    /// <remarks>
    /// Deriving classes are also expected to send the <see cref="Deleted"/> message to notify other observers
    /// or pages that the object has been deleted, so they can respond accordingly.
    /// </remarks>
    [RelayCommand]
    // ReSharper disable once UnusedMemberInSuper.Global the command is though so don't remove.
    protected virtual Task Delete() => Task.FromResult(Messenger.Send(new Deleted(this)));

    /// <summary>
    /// A command to duplicate the <see cref="Observer"/> object in the database and UI. The default
    /// implementation does nothing and not all observers may need this, but it will be supported by more than one so
    /// this is to consolidate the code. 
    /// </summary>
    /// <returns>The <see cref="Task"/> representing the async function to perform.</returns>
    [RelayCommand]
    protected virtual Task Duplicate() => Task.CompletedTask;

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    protected virtual Task Export() => Task.CompletedTask;

    /// <inheritdoc />
    protected override Task Navigate() => Navigator.Navigate(this);

    /// <summary>
    /// A command that updates the name of the underlying item model and sends a command to update the persisted value.
    /// </summary>
    /// <param name="name">The new name of the item.</param>
    [RelayCommand]
    private async Task Rename(string? name)
    {
        if (string.IsNullOrEmpty(name))
        {
            name = await Prompter.PromptRename(this);
        }

        if (string.IsNullOrEmpty(name)) return;

        var result = await UpdateName(name);
        if (result.IsFailed) return;

        Name = name;
        Messenger.Send(new Renamed(this));
        IsEditing = false;
        IsNew = false;
    }

    /// <summary>
    /// Command to activate the <see cref="IsEditing"/> indication for this item to indicate to the UI that this item
    /// is being renamed.
    /// </summary>
    [RelayCommand]
    private void StartEdit()
    {
        IsEditing = true;
    }

    /// <summary>
    /// A command to reset the <see cref="IsEditing"/> indication to notify property change the <see cref="Name"/>
    /// of this item to refresh the UI.
    /// </summary>
    [RelayCommand]
    private void EndEdit()
    {
        IsEditing = false;
        OnPropertyChanged(nameof(Name));
    }

    #endregion

    #region Handlers

    /// <summary>
    /// Handles the observer renamed message by updating this object's name if it is not the sender of the
    /// message. This will keep the observer instances in sync.
    /// </summary>
    public virtual void Receive(Renamed message)
    {
        var other = message.Observer;

        //Only applies to other observers with same identity.
        if (Id != other.Id) return;

        //To sync other observers check that the name is updated.
        if (Name != other.Name)
        {
            Name = other.Name;
            /*Messenger.Send(new Renamed(this));*/
        }

        //Always notify property changed to sync the UI.
        OnPropertyChanged(nameof(Name));
    }

    #endregion

    #region Messages

    /// <summary>
    /// A message that indicates a new observer was created. This can be sent from various places and handled in pages
    /// that need to respond/update to reflect the newly created observer
    /// </summary>
    /// <param name="Observer">The observer that was created.</param>
    public record Created(Observer Observer);

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
    /// A message that indicates the observer was duplicated and a new instance was created. This can be handled by
    /// parent observers or pages to refresh the UI to show the newly duplicated instance.
    /// </summary>
    /// <param name="Source">The original observer instance that was the source of duplication.</param>
    /// <param name="Duplicate">The new observer instance that represents the duplicate.</param>
    public record Duplicated(Observer Source, Observer Duplicate);

    /// <summary>
    /// A request for an in memory instance of an observer that has an id equal to the provided id, or a child observer
    /// equal to the provided id, depending on how the recipient logic is implemented.
    /// </summary>
    /// <param name="id">The observer id which either requested the instance or is the id of the instance to return.</param>
    /// <typeparam name="TObserver">The instance of the observer that matches the request criteria.</typeparam>
    public class Request<TObserver>(Guid id) : RequestMessage<TObserver> where TObserver : Observer
    {
        public Guid Id { get; } = id;
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
    /// Gets the configured collection of <see cref="MenuActionItem"/> to display in the UI.
    /// </summary>
    protected virtual IEnumerable<MenuActionItem> GenerateMenuItems() => Enumerable.Empty<MenuActionItem>();

    /// <summary>
    /// Gets the configured collection of <see cref="MenuActionItem"/> to display in the UI.
    /// </summary>
    protected virtual IEnumerable<MenuActionItem> GenerateContextItems() => Enumerable.Empty<MenuActionItem>();

    /// <summary>
    /// This will force the editing mode to end when the item loses selection.
    /// </summary>
    partial void OnIsSelectedChanged(bool value)
    {
        if (IsEditing && !value)
        {
            IsEditing = false;
        }
    }
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