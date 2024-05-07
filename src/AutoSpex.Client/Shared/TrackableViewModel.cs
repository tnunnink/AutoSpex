using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace AutoSpex.Client.Shared;

/// <summary>
/// A marker interface to allow me to pass instance of <see cref="Observer{TModel}"/> around with knowledge that it
/// implements these interfaces.
/// </summary>
public interface ITrackable : INotifyPropertyChanged, IChangeTracking;

/// <summary>
/// 
/// </summary>
public abstract class TrackableViewModel : ViewModelBase, ITrackable
{
    private readonly HashSet<string> _changed = [];
    private readonly HashSet<string> _tracking = [];
    private readonly List<ITrackable> _tracked = [];

    /// <summary>
    /// Indicates that there are changes made to the state of the view model that need to be persisted or saved.
    /// </summary>
    public virtual bool IsChanged => _changed.Count > 0 || _tracked.Any(t => t.IsChanged);

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
    /// Accepts the changes to the specified property name. This will just remove the property from the internal
    /// change collection.
    /// </summary>
    /// <param name="propertyName">The name of the property to clear from the change list.</param>
    protected void AcceptChanges(string propertyName)
    {
        _changed.Remove(propertyName);
        OnPropertyChanged(nameof(IsChanged));
    }

    /// <summary>
    /// Refreshes all bindings to the derived observer object. This could be used if changes are made to the model
    /// internally without the observer knowing (data refresh or domain level logic/events) It should signify a reset or
    /// sync between the model and observer and therefore UI. 
    /// </summary>
    public virtual void Refresh()
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
    /// view model so to propagate changes up the object graph.
    /// </summary>
    /// <param name="trackable">The child <see cref="ITrackable"/> to be tracked.</param>
    /// <exception cref="ArgumentNullException"><paramref name="trackable"/> is null.</exception>
    protected void Track(ITrackable trackable)
    {
        ArgumentNullException.ThrowIfNull(trackable);
        _tracked.Add(trackable);
        trackable.PropertyChanged += OnTrackedModelPropertyChanged;
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
        var removed = _tracked.Remove(trackable);
        if (!removed) return;
        trackable.PropertyChanged -= OnTrackedModelPropertyChanged;
    }

    /// <inheritdoc />
    protected override void OnDeactivated()
    {
        base.OnDeactivated();

        foreach (var tracked in _tracked)
        {
            tracked.PropertyChanged -= OnTrackedModelPropertyChanged;
        }

        _tracked.Clear();
    }

    /// <summary>
    /// We can override the base changed method to avoid wiring up an event handler. This calls the base implementation
    /// and then adds the property to the changed list if it is note ignored, not empty, null, or not IsChanged itself.
    /// </summary>
    /// <param name="e">The property changed event args.</param>
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (string.IsNullOrEmpty(e.PropertyName)
            || !_tracking.Contains(e.PropertyName)
            || e.PropertyName == nameof(IsChanged)) return;

        _changed.Add(e.PropertyName);
        OnPropertyChanged(nameof(IsChanged));
    }

    private void OnTrackedModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(IsChanged)) return;
        OnPropertyChanged(nameof(IsChanged));
    }
}