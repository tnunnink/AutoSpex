using System;
using AutoSpex.Client.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentResults;

namespace AutoSpex.Client.Shared;

/// <summary>
/// <para>
/// A wrapper for our engine models types so that we can expose and decorate the objects with UI related functionality.
/// This class implements <see cref="ObservableValidator"/> which gives it the notification needed to sync the UI, as well
/// as the ability to use property validation for inputs.
/// </para>
/// <para>
/// This class also offers the application <see cref="Messenger"/> for sending messages to parent pages in a loosely
/// coupled fashion. It also offers a simple change tracking implementation which we can use to determine if a model
/// has been updated and requires a save changes to be sent.
/// </para>
/// </summary>
/// <param name="model">The model instance to wrap.</param>
/// <typeparam name="TModel">The model type to wrap.</typeparam>
public abstract partial class Observer<TModel>(TModel model) : TrackableViewModel
{
    /// <summary>
    /// The underlying model object that is being wrapped by the observer.
    /// </summary>
    public TModel Model { get; } = model ?? throw new ArgumentNullException(nameof(model));

    /// <summary>
    /// A command to issue deletion of this <see cref="Observer{TModel}"/> object from the database.
    /// </summary>
    /// <returns>The <see cref="Result{TValue}"/> of the deletion command.</returns>
    /// <remarks>The default is no implementation. Deriving observers can implement as needed.</remarks>
    [RelayCommand]
    protected virtual Task Delete() => Task.CompletedTask;

    /// <inheritdoc />
    public override async Task Navigate()
    {
        //todo I think we need to get some navigation action from the settings for the user so they can decide by default what this does (open or replace)
        await Navigator.Navigate(this);
    }
    
    [RelayCommand]
    private async Task OpenInTab()
    {
        await Navigator.Navigate(this, NavigationAction.Open);
    }
}