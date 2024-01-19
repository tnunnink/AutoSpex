using System;
using System.Collections.Generic;
using System.ComponentModel;
using AutoSpex.Client.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Observers;

public abstract class Observer<TModel>(TModel model) : ObservableValidator, IChangeTracking
{
    private readonly HashSet<string> _changed = [];
    private readonly HashSet<string> _ignore = [];
    
    /// <summary>
    /// The underlying model object that is being wrapped by the observer.
    /// </summary>
    public TModel Model { get; } = model ?? throw new ArgumentNullException(nameof(model));

    /// <summary>
    /// Indicates that there are changes made to the underlying model object that need to be persisted or saved.
    /// </summary>
    public virtual bool IsChanged => _changed.Count > 0;
    
    /// <summary>
    /// The application messenger which can be used to send messages for events to the observer.
    /// </summary>
    protected IMessenger Messenger { get; } = Container.Resolve<IMessenger>();
    
    /// <summary>
    /// The application navigator which can be used to send navigation requests that are received by containing pages.
    /// </summary>
    protected Navigator Navigator { get; } = Container.Resolve<Navigator>();

    /// <summary>
    /// Refreshes all bindings to the derived observer object. This could be used if changes are made to the model
    /// internally without the observer knowing (data refresh or domain level logic/events) It should signify a reset or
    /// sync between the the model and observer and UI. 
    /// </summary>
    public virtual void Refresh()
    {
        _changed.Clear();
        OnPropertyChanged(string.Empty);
    }

    /// <summary>
    /// Clears the list of changes and calls the AcceptChanges method for each tracked object.
    /// </summary>
    public virtual void AcceptChanges()
    {
        _changed.Clear();
    }

    /// <summary>
    /// Adds a property name to the list of properties to ignore changes for.
    /// </summary>
    /// <param name="propertyName">The name of the property to ignore changes for.</param>
    public void Ignore(string propertyName) => _ignore.Add(propertyName);

    /// <summary>
    /// We can override the base changed method to avoid wiring up an event handler. This calls the base implementation
    /// and then sets IsChanged to true;
    /// </summary>
    /// <param name="e"></param>
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (string.IsNullOrEmpty(e.PropertyName) || _ignore.Contains(e.PropertyName) ||
            e.PropertyName == nameof(IsChanged)) return;
        
        _changed.Add(e.PropertyName);
        OnPropertyChanged(nameof(IsChanged));
    }
}