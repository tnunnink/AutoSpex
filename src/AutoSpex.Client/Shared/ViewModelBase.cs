using System;
using System.Collections.Generic;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MediatR;

namespace AutoSpex.Client.Shared;

/// <summary>
/// Base class for view models in the application.
/// </summary>
public abstract partial class ViewModelBase : ObservableValidator, IChangeTracking, IDisposable
{
    //Instead of DI I'm resolving these dependencies for every view model directly.
    //I am choosing to do this to simplify the construction of child view models, and since I am never really
    //mocking these dependencies, but rather using the real implementations to preform integration testing for my application.
    protected readonly IMessenger Messenger = Container.Resolve<IMessenger>();
    protected readonly IMediator Mediator = Container.Resolve<IMediator>();

    private readonly List<ViewModelBase> _tracked = new();

    /// <summary>
    /// Gets or sets a value indicating whether the object's state has changed.
    /// </summary>
    /// <value>
    /// <c>true</c> if the object's state has changed; otherwise, <c>false</c>.
    /// </value>
    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private bool _isChanged;

    /// <summary>
    /// Loads the data needed for the view model.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [RelayCommand]
    protected virtual Task Load()
    {
        return Task.CompletedTask;
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    protected virtual Task Save()
    {
        return Task.CompletedTask;
    }

    protected virtual bool CanSave() => true;
    
    

    /// <summary>
    /// Accepts the changes made to the object and resets the IsChanged flag to false.
    /// </summary>
    public void AcceptChanges()
    {
        SetIsChanged(false);

        foreach (var tracked in _tracked)
        {
            tracked.AcceptChanges();
        }
    }

    /// <summary>
    /// Subscribes the RaiseIsChanged method to the PropertyChanged event.
    /// </summary>
    protected void Track()
    {
        PropertyChanged += RaiseIsChanged;
    }

    /// <summary>
    /// Track method is used to track changes in the specified ViewModelBase object.
    /// When any property in the ViewModelBase object is changed, the RaiseIsChanged event will be raised and the ViewModelBase object will be added to the list of tracked objects (_tracked
    /// ).
    /// </summary>
    /// <param name="viewModel">The ViewModelBase object to be tracked.</param>
    protected void Track(ViewModelBase viewModel)
    {
        viewModel.PropertyChanged += RaiseIsChanged;
        _tracked.Add(viewModel);
    }

    /// <summary>
    /// Removes the given view model from the list of tracked view models and stops listening to its PropertyChanged event.
    /// </summary>
    /// <param name="viewModel">The view model to forget.</param>
    protected void Forget(ViewModelBase viewModel)
    {
        viewModel.PropertyChanged -= RaiseIsChanged;
        _tracked.Remove(viewModel);
    }

    /// <summary>
    /// Releases the resources used by the current instance.
    /// </summary>
    public virtual void Dispose()
    {
        PropertyChanged -= RaiseIsChanged;

        foreach (var viewModel in _tracked)
        {
            viewModel.PropertyChanged -= RaiseIsChanged;
        }

        Messenger.UnregisterAll(this);
        GC.SuppressFinalize(this);
    }

    private void RaiseIsChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (ReferenceEquals(sender, this) && e.PropertyName == nameof(IsChanged)) return;
        SetIsChanged(true);
    }

    private void SetIsChanged(bool value)
    {
        if (IsChanged == value) return;
        IsChanged = value;
        OnPropertyChanged(nameof(IsChanged));
    }
}