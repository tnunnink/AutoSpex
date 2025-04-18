﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace AutoSpex.Client.Shared;

/// <summary>
/// A marker interface to allow me to pass instance of <see cref="Observer{TModel}"/> around with knowledge that it
/// implements these interfaces.
/// </summary>
public interface ITrackable : INotifyPropertyChanged, IChangeTracking, INotifyDataErrorInfo, IDisposable
{
    /// <summary>
    /// Performs validation of all properties for the view model to ensure valid state of the data.
    /// </summary>
    /// <remarks>
    /// This will call the underlying ValidateAllProperties using the built-in ObservableValidator.
    /// </remarks>
    IEnumerable<ValidationResult> Validate();
}

/// <summary>
/// A base view model that implements tracking of the child properties and objects and provides functionality to indicate
/// to the UI that changes exists for this view model.
/// </summary>
public abstract class TrackableViewModel : ViewModelBase, ITrackable
{
    private readonly HashSet<string> _changed = [];
    private readonly HashSet<string> _tracking = [];
    private readonly List<ITrackable> _tracked = [];
    private readonly List<IDisposable> _disposables = [];

    /// <summary>
    /// Indicates that there are changes made to the state of the view model that need to be persisted or saved.
    /// </summary>
    public virtual bool IsChanged => _changed.Count > 0 || _tracked.Any(t => t.IsChanged);

    /// <summary>
    /// Indicates that this view model or any of its tracked view models have errors present.
    /// </summary>
    public virtual bool IsErrored => HasErrors || _tracked.Any(t => t.HasErrors);

    /// <summary>
    /// Accepts the changes made to the trackable view model and all its tracked view models.
    /// </summary>
    public void AcceptChanges()
    {
        foreach (var trackable in _tracked)
            trackable.AcceptChanges();

        _changed.Clear();

        OnPropertyChanged(nameof(IsChanged));
    }

    /// <summary>
    /// Refreshes all bindings to the derived observer object. This could be used if changes are made to the model
    /// internally without the observer knowing (data refresh or domain level logic/events) It should signify a reset
    /// between the model and observer and therefore UI. 
    /// </summary>
    public void Refresh()
    {
        _changed.Clear();
        OnPropertyChanged(string.Empty);
    }

    /// <summary>
    /// Adds a property name to the list of properties to track changes for.
    /// </summary>
    /// <param name="propertyName">The name of the property to track changes for.</param>
    /// <remarks>
    /// When the property changed even fires and the property is included in the underlying tracked property list,
    /// this will add the property to the list of changes and notify property changed for the <see cref="IsChanged"/> property.
    /// </remarks>
    protected void Track(string propertyName) => _tracking.Add(propertyName);

    /// <summary>
    /// Adds the provided <see cref="ITrackable"/> to the internal collection of tracked objects for this
    /// view model so to both deactive on parent deactivation (release memory) and optionally propagate
    /// changes up the object graph.
    /// </summary>
    /// <param name="trackable">The child <see cref="ITrackable"/> to be tracked.</param>
    /// <exception cref="ArgumentNullException"><paramref name="trackable"/> is null.</exception>
    protected void Track(ITrackable trackable)
    {
        ArgumentNullException.ThrowIfNull(trackable);

        _tracked.Add(trackable);

        trackable.PropertyChanged -= OnTrackedModelPropertyChanged;
        trackable.ErrorsChanged -= OnTrackedModelErrorsChanged;
        trackable.PropertyChanged += OnTrackedModelPropertyChanged;
        trackable.ErrorsChanged += OnTrackedModelErrorsChanged;
    }

    /// <summary>
    /// Removes the provided trackable from the internal collection of tracked objects for this parent trackable, which
    /// will unsubscribe the property changed event in order to clean up the reference to the child. This should
    /// be done of any child that is expected to have a shorter lifetime than this parent.
    /// </summary>
    /// <param name="trackable">The child <see cref="ITrackable"/> to forget.</param>
    /// <exception cref="ArgumentNullException"><paramref name="trackable"/> is null.</exception>
    protected void Forget(ITrackable trackable)
    {
        ArgumentNullException.ThrowIfNull(trackable);
        _tracked.Remove(trackable);
        trackable.PropertyChanged -= OnTrackedModelPropertyChanged;
        trackable.ErrorsChanged -= OnTrackedModelErrorsChanged;
    }

    /// <summary>
    /// Registers a disposable object to be disposed of when the view model is closed or deactivated.
    /// </summary>
    /// <param name="disposable">The disposable object to be registered for disposal.</param>
    protected void RegisterDisposable(IDisposable? disposable)
    {
        if (disposable is null) return;
        _disposables.Add(disposable);
    }

    /// <summary>
    /// Disposes/deactivates all tracked or registered view models for this page.
    /// This action can be taken before a page needs to be reloaded so that all current instances can be released or
    /// notify that they are ready to be disposed.
    /// </summary>
    public void Flush()
    {
        ReleaseTrackedModels();
        DisposeModels();
    }

    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate()
    {
        var errors = new List<ValidationResult>();

        ValidateAllProperties();
        errors.AddRange(GetErrors());

        foreach (var trackable in _tracked)
        {
            errors.AddRange(trackable.Validate());
        }

        return errors;
    }

    /// <inheritdoc />
    protected override void OnDeactivated()
    {
        base.OnDeactivated();
        ReleaseTrackedModels();
        DisposeModels();
        Dispose();
    }

    /// <summary>
    /// We can override the base changed method to avoid wiring up an event handler. This calls the base implementation
    /// and then adds the property to the changed list if it is not ignored, not empty, null, or not IsChanged itself.
    /// </summary>
    /// <param name="e">The property changed event args.</param>
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (string.IsNullOrEmpty(e.PropertyName)) return;

        switch (e.PropertyName)
        {
            case nameof(IsChanged) or nameof(IsErrored):
                return;
            case nameof(HasErrors):
                OnPropertyChanged(nameof(IsErrored));
                return;
        }

        if (!_tracking.Contains(e.PropertyName)) return;
        _changed.Add(e.PropertyName);
        OnPropertyChanged(nameof(IsChanged));
    }

    /// <summary>
    /// Forwards the tracked model property changed event up the chain to notify this observer the <see cref="IsChanged"/>
    /// has changed.
    /// </summary>
    private void OnTrackedModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(IsChanged)) return;
        OnPropertyChanged(nameof(IsChanged));
    }

    /// <summary>
    /// Forwards the tracked model errors changed event up the chain to notify this observer the <see cref="IsErrored"/>
    /// has changed.
    /// </summary>
    private void OnTrackedModelErrorsChanged(object? sender, DataErrorsChangedEventArgs e)
    {
        OnPropertyChanged(nameof(IsErrored));
    }

    /// <summary>
    /// Releases all tracked models and deactivates child view models, allowing for the release of memory.
    /// </summary>
    /// <remarks>
    /// This method is called when the view model is deactivated to ensure that all tracked models and child view models are released.
    /// Tracked models are unsubscribed from property change and error change events, and deactivate andy child models.
    /// </remarks>
    private void ReleaseTrackedModels()
    {
        foreach (var trackable in _tracked)
        {
            trackable.PropertyChanged -= OnTrackedModelPropertyChanged;
            trackable.ErrorsChanged -= OnTrackedModelErrorsChanged;
            trackable.Dispose();
        }

        _tracked.Clear();
    }

    /// <summary>
    /// Disposes all the models that were tracked by the view model.
    /// </summary>
    /// <remarks>
    /// This method iterates through the list of disposables that were registered during the lifetime of the view model
    /// and calls dispose on each of them. Once disposed, the disposables are removed from the internal disposables list.
    /// </remarks>
    private void DisposeModels()
    {
        foreach (var disposable in _disposables)
        {
            disposable.Dispose();
        }

        _disposables.Clear();
    }

    /// <summary>
    /// Sets the IsActive property to false and suppresses the finalization of the object.
    /// </summary>
    public void Dispose()
    {
        if (IsActive)
        {
            IsActive = false;
        }

        GC.SuppressFinalize(this);
    }
}