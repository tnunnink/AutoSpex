using System;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Shared;

/// <summary>
/// <para>
/// A wrapper for our engine models types so that we can expose and decorate the objects with UI related functionality.
/// This class implements <see cref="TrackableViewModel"/> which gives it the notification needed to sync the UI, as well
/// as the ability to use property validation for inputs.
/// </para>
/// </summary>
/// <typeparam name="TModel">The model type to wrap.</typeparam>
public abstract partial class Observer<TModel> : TrackableViewModel, IEquatable<Observer<TModel>>
{
    /// <summary>
    /// Creates a new <see cref="Observer{TModel}"/> with the provided model object.
    /// </summary>
    /// <param name="model">The model instance to wrap.</param>
    /// <typeparam name="TModel">The model type to wrap.</typeparam>
    protected Observer(TModel model)
    {
        Model = model ?? throw new ArgumentNullException(nameof(model));
        IsActive = true;
    }

    /// <summary>
    /// A <see cref="Guid"/> that uniquely identifies this observer. This should be the same for each instance wrapping
    /// the same underlying model. By default, this just creates a new <see cref="Guid"/> but deriving classes will
    /// implement to point to the correct model id. 
    /// </summary>
    public virtual Guid Id { get; } = Guid.NewGuid();

    /// <summary>
    /// The underlying model object that is being wrapped by the observer.
    /// </summary>
    public TModel Model { get; }

    /// <summary>
    /// A command to issue deletion of this <see cref="Observer{TModel}"/> object from the database.
    /// </summary>
    /// <returns>The <see cref="Task"/> representing the async function to perform.</returns>
    /// <remarks>
    /// Deriving classes are also expected to send the <see cref="Deleted"/> message to notify other observers
    /// or pages that the object has been deleted so they can respond accordingly.
    /// </remarks>
    [RelayCommand]
    // ReSharper disable once UnusedMemberInSuper.Global the command is though so don't remove.
    protected virtual Task Delete() => Task.FromResult(Messenger.Send(new Deleted(this)));

    /// <summary>
    /// A command to duplicate the <see cref="Observer{TModel}"/> object in the database and UI. The default
    /// implementation does nothing and not all observers may need this but it will be supported by more than one so
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

    public bool Equals(Observer<TModel>? other)
    {
        if (ReferenceEquals(this, other)) return true;
        return other is not null && other.Id == Id;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj)) return true;
        return obj is Observer<TModel> other && other.Id == Id;
    }

    public override int GetHashCode() => Id.GetHashCode();
    public static bool operator ==(Observer<TModel> first, Observer<TModel> second) => Equals(first, second);
    public static bool operator !=(Observer<TModel> first, Observer<TModel> second) => !Equals(first, second);

    public record Created(Observer<TModel> Observer);

    public record Deleted(Observer<TModel> Observer);
}