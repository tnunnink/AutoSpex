using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace AutoSpex.Client.Shared;

public class ObserverCollection<TModel, TObserver> : ObservableCollection<TObserver>, ITrackable
    where TObserver : Observer<TModel>
{
    private bool _changed;
    private bool _refreshing;
    private readonly Func<ICollection<TObserver>> _refresh;
    private readonly Action<int, TModel>? _add;
    private readonly Action<int, TModel>? _insert;
    private readonly Action<int, TModel>? _remove;
    private readonly Action? _clear;

    public ObserverCollection()
    {
        _refresh = () => Enumerable.Empty<TObserver>().ToList();
    }

    public ObserverCollection(Func<ICollection<TObserver>> refresh,
        Action<int, TModel>? add = default,
        Action<int, TModel>? insert = default,
        Action<int, TModel>? remove = default,
        Action? clear = default) : base(refresh())
    {
        _refresh = refresh;
        _add = add;
        _insert = insert;
        _remove = remove;
        _clear = clear;

        Attach(this);
    }

    public ObserverCollection(IList<TModel> models, Func<TModel, TObserver> wrapper) : base(Initialize(models, wrapper))
    {
        _refresh = () => models.Select(wrapper).ToList();
        _add = models.Insert;
        _insert = models.Insert;
        _remove = (i, _) => models.RemoveAt(i);
        _clear = models.Clear;

        Attach(this);
    }

    public bool IsChanged => _changed || this.Any(o => o.IsChanged);
    public bool IsRefreshing { get; private set; }
    public bool HasErrors => this.Any(o => o.IsErrored);
    public event PropertyChangedEventHandler? ItemPropertyChanged;
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public void AcceptChanges()
    {
        _changed = false;

        foreach (var item in this)
        {
            item.AcceptChanges();
        }

        OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsChanged)));
    }

    public void AddRange(IEnumerable<TObserver> observers)
    {
        foreach (var observer in observers)
        {
            Add(observer);
        }
    }

    /// <summary>
    /// Refreshes the state of this <see cref="ObserverCollection{TModel,TObserver}"/> to the underlying model collection
    /// and clears any changes to this collection. 
    /// </summary>
    /// <param name="observers">
    /// Optional set of observer items to sync this collection to.
    /// If not provided uses the internal refresh callback.
    /// If neither produce items, then the collection is cleared.
    /// </param>
    public void Refresh(IEnumerable<TObserver>? observers = default)
    {
        _refreshing = true;

        ClearItems();

        var collection = observers ?? _refresh();
        
        foreach (var observer in collection)
            Add(observer);

        foreach (var observer in this)
            observer.Refresh();

        _refreshing = false;
        _changed = false;
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsChanged)));
    }

    /// <summary>
    /// Sync will find all changes in the underlying collection and update this <see cref="ObserverCollection{TModel,TObserver}"/>
    /// to match. This will in turn raise the collection change and notify of changes to the collection. This is in contrast to
    /// <see cref="Refresh"/> which syncs the collection but clears changes.
    /// </summary>
    /// <param name="observers">
    /// Optional set of observer items to sync this collection to.
    /// If not provided uses the internal refresh callback.
    /// If neither produce items, then nothing happens.
    /// </param>
    public void Sync(IEnumerable<TObserver>? observers = default)
    {
        var collection = (observers ?? _refresh()).ToList();

        var added = collection.Where(x => this.All(o => o != x)).ToList();
        var removed = this.Where(o => collection.All(x => x != o)).ToList();
        var modified = this.Except(added).Except(removed).Where(x => x.IsChanged).ToList();

        foreach (var observer in added)
            Add(observer);

        foreach (var observer in removed)
            Remove(observer);

        foreach (var observer in modified)
            observer.Refresh();
    }

    /// <summary>
    /// Sorts this <see cref="ObserverCollection{TModel,TObserver}"/> using the provided selector and comparer.
    /// </summary>
    /// <param name="selector">A function to select the property to sort on.</param>
    /// <param name="comparer">A comparer used to determine the sort order.</param>
    /// <typeparam name="TField">The type of the selected property.</typeparam>
    public void Sort<TField>(Func<TObserver, TField> selector, IComparer<TField>? comparer)
    {
        var sorted = this.OrderBy(selector, comparer).ToList();

        for (var i = 0; i < sorted.Count; i++)
            Move(IndexOf(sorted[i]), i);

        _changed = false;
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsChanged)));
    }

    /// <summary>
    /// Retrieves all error messages found for child observers in the <see cref="ObserverCollection{TModel,TObserver}"/>
    /// </summary>
    /// <param name="propertyName">The name of the property to get errors for.</param>
    /// <returns>An <see cref="IEnumerable"/> containing the list of errors.</returns>
    public IEnumerable GetErrors(string? propertyName)
    {
        return this.SelectMany(o => o.GetErrors(propertyName));
    }

    /// <inheritdoc />
    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnCollectionChanged(e);

        //Don't notify change when refreshing since it is intended to sync to the underlying collection.
        if (IsRefreshing) return;

        //When the collection changes just set a flag to indicate for IsChanged.
        _changed = true;
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsChanged)));
    }

    /// <inheritdoc />
    protected override void InsertItem(int index, TObserver observer)
    {
        base.InsertItem(index, observer);
        Attach(observer);

        //Abort if refreshing to avoid circular calls.
        if (IsRefreshing) return;

        if (index == Count - 1)
        {
            _add?.Invoke(index, observer.Model);
            return;
        }

        _insert?.Invoke(index, observer.Model);
    }

    /// <inheritdoc />
    protected override void RemoveItem(int index)
    {
        var observer = this[index];
        base.RemoveItem(index);
        Detach(observer);

        //Abort if refreshing to avoid circular calls.
        if (IsRefreshing) return;
        _remove?.Invoke(index, observer.Model);
    }

    /// <summary>Removes all items from the collection.</summary>
    protected override void ClearItems()
    {
        base.ClearItems();
        Detach(this);

        //Abort if refreshing to avoid circular calls.
        if (IsRefreshing) return;
        _clear?.Invoke();
    }

    /// <summary>
    /// Creates the initial observer collection to be passed up to the base class using the provided wrapper function.
    /// </summary>
    private static IEnumerable<TObserver> Initialize(ICollection<TModel> models, Func<TModel, TObserver> wrapper)
    {
        if (models is null) throw new ArgumentNullException(nameof(models));
        if (wrapper is null) throw new ArgumentNullException(nameof(wrapper));
        return models.Select(wrapper);
    }

    /// <summary>
    /// Attaches change and error handlers to the child observers to propagate notifications up the tree.
    /// </summary>
    private void Attach(IEnumerable<TObserver> observers)
    {
        foreach (var observer in observers)
            Attach(observer);
    }

    /// <summary>
    /// Attaches change and error handlers to a child observer to propagate notifications up the tree.
    /// </summary>
    private void Attach(TObserver observer)
    {
        observer.PropertyChanged -= OnObserverPropertyChanged;
        observer.PropertyChanged += OnObserverPropertyChanged;

        observer.ErrorsChanged -= OnObserverErrorsChanged;
        observer.ErrorsChanged += OnObserverErrorsChanged;
    }

    /// <summary>
    /// Detaches change and error handlers from the observers to release reference.
    /// </summary>
    private void Detach(IEnumerable<TObserver> observers)
    {
        foreach (var observer in observers)
            Detach(observer);
    }

    /// <summary>
    /// Detaches change and error handlers from the observer to release reference.
    /// </summary>
    private void Detach(TObserver observer)
    {
        observer.PropertyChanged -= OnObserverPropertyChanged;
        observer.ErrorsChanged -= OnObserverErrorsChanged;
    }

    /// <summary>
    /// When a child observer property changes, we want to invoke the local ItemPropertyChanged event and raise the
    /// IsChanged property changed to notify of changes in the collection.
    /// </summary>
    private void OnObserverPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not TObserver observer) return;

        //Raise that an item property changed.
        ItemPropertyChanged?.Invoke(observer, e);

        //Propagate the IsChanged property change up the object graph.
        if (e.PropertyName == nameof(IsChanged))
        {
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsChanged)));
        }
    }

    /// <summary>
    /// When a child observer errors changed we want to propagate that event up the type graph.
    /// </summary>
    private void OnObserverErrorsChanged(object? sender, DataErrorsChangedEventArgs e)
    {
        if (sender is not TObserver observer) return;
        ErrorsChanged?.Invoke(observer, e);
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(HasErrors)));
    }
}

/// <summary>
/// Some helpers for the base <see cref="ObservableCollection{T}"/> class to make adding and refreshing it's items easier.
/// </summary>
public static class ObservableCollectionExtensions
{
    public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            collection.Add(item);
        }
    }

    public static void Refresh<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
    {
        collection.Clear();

        foreach (var item in items)
        {
            collection.Add(item);
        }
    }
}