using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace AutoSpex.Client.Shared;

public sealed class ObserverCollection<TModel, TObserver> : ObservableCollection<TObserver>, ITrackable
    where TObserver : Observer<TModel>
{
    private bool _changed;
    private bool _refreshing;
    private List<TModel> _models = [];
    private Func<ICollection<TObserver>> _refresh;
    private Action<int, TModel>? _add;
    private Action<int, TModel>? _insert;
    private Action<int, TModel>? _remove;
    private Action<int, int>? _move;
    private Action? _clear;
    private Func<int>? _count;

    public ObserverCollection()
    {
        _refresh = () => Enumerable.Empty<TObserver>().ToList();
    }

    public ObserverCollection(Func<ICollection<TObserver>> refresh,
        Action<int, TModel>? add = default,
        Action<int, TModel>? insert = default,
        Action<int, TModel>? remove = default,
        Action<int, int>? move = default,
        Action? clear = default,
        Func<int>? count = default) : base(refresh())
    {
        _refresh = refresh;
        _add = add;
        _insert = insert;
        _remove = remove;
        _move = move;
        _clear = clear;
        _count = count;

        Attach(this);
    }

    public ObserverCollection(IList<TModel> models, Func<TModel, TObserver> wrapper) : base(Initialize(models, wrapper))
    {
        _refresh = () => models.Select(wrapper).ToList();
        _add = (_, m) => models.Add(m);
        _insert = models.Insert;
        _remove = (_, m) => models.Remove(m);
        _move = models.Move;
        _count = () => models.Count;

        Attach(this);
    }

    public bool IsChanged => _changed || this.Any(o => o.IsChanged);
    public bool HasErrors => this.Any(o => o.IsErrored);
    public bool HasItems => _count is not null && _count() > 0;
    public event PropertyChangedEventHandler? ItemPropertyChanged;
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;


    /// <inheritdoc />
    public void AcceptChanges()
    {
        _changed = false;

        foreach (var item in this)
        {
            item.AcceptChanges();
        }

        OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsChanged)));
    }

    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate()
    {
        var errors = new List<ValidationResult>();

        foreach (var observer in this)
        {
            var results = observer.Validate();
            errors.AddRange(results);
        }

        return errors;
    }

    /// <summary>
    /// Binds this observer collection to the provided list using the specified wrapper function.
    /// </summary>
    /// <param name="models">The list of models to bind to the ObserverCollection</param>
    /// <param name="wrapper">A function that wraps a model into its corresponding observer</param>
    public void Bind(IList<TModel> models, Func<TModel, TObserver> wrapper)
    {
        _refresh = () => models.Select(wrapper).ToList();
        _add = (_, m) => models.Add(m);
        _insert = models.Insert;
        _remove = (_, m) => models.Remove(m);
        _move = models.Move;
        _clear = models.Clear;
        _count = () => models.Count;

        RefreshCollection(_refresh());
    }

    /// <summary>
    /// Binds this observer collection to the provided list of observer objects.
    /// This does not bind any mutation methods (add, remove, etc.) as it is only expected to be read from and filtered
    /// but never changed.
    /// </summary>
    /// <param name="observers">The list of models to bind to the ObserverCollection</param>
    public void BindReadOnly(IList<TObserver> observers)
    {
        _refresh = () => observers;
        _count = () => observers.Count;
        RefreshCollection(_refresh());
    }

    /// <summary>
    /// Tries to add the specified observer to the <see cref="ObserverCollection{TModel, TObserver}"/>.
    /// If an observer with the same ID already exists in the collection, it will not be added.
    /// </summary>
    /// <param name="observer">The observer to add to the collection.</param>
    /// <returns>
    /// <see langword="true"/> if the observer was added to the collection;
    /// <see langword="false"/> if an observer with the same ID already exists in the collection.</returns>
    public bool TryAdd(TObserver observer)
    {
        if (this.Any(x => x.Id == observer.Id)) return false;
        Add(observer);
        return true;
    }

    /// <summary>
    /// Removes any observer items from the collection that match the specified predicate.
    /// </summary>
    /// <param name="predicate">The function used to determine if an observer item should be removed.</param>
    public bool RemoveAny(Func<TObserver, bool> predicate)
    {
        var targets = this.Where(predicate).ToList();

        foreach (var target in targets)
        {
            Remove(target);
        }

        return targets.Count > 0;
    }

    /// <summary>
    /// Checks if this collection contains the provided observer instance using referential equality. 
    /// </summary>
    /// <param name="observer">The observer to check for equality.</param>
    /// <returns>True if any observer in the collection is equal to the specified observer; otherwise, false.</returns>
    public bool Has(Observer observer)
    {
        return this.Any(x => x.Is(observer));
    }

    /// <summary>
    /// Refreshes the state of this <see cref="ObserverCollection{TModel,TObserver}"/> to the underlying
    /// model collection by invoking the refresh delegate. 
    /// </summary>
    public void Refresh() => RefreshCollection(_refresh());

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
        var others = this.Except(added).Except(removed).ToList();

        foreach (var observer in added)
            Add(observer);

        foreach (var observer in removed)
            Remove(observer);

        foreach (var observer in others)
            observer.Refresh();
    }

    /// <summary>
    /// Sorts this <see cref="ObserverCollection{TModel,TObserver}"/> using the provided selector and comparer.
    /// </summary>
    /// <param name="selector">A function to select the property to sort on.</param>
    /// <param name="comparer">A comparer used to determine the sort order.</param>
    /// <typeparam name="TField">The type of the selected property.</typeparam>
    public void Sort<TField>(Func<TObserver, TField> selector, IComparer<TField>? comparer = default)
    {
        _refreshing = true;

        var sorted = this.OrderBy(selector, comparer).ToList();

        for (var i = 0; i < sorted.Count; i++)
            Move(IndexOf(sorted[i]), i);

        _refreshing = false;
    }

    /// <summary>
    /// Uses the provided filter function to get a subset of the underlying model collection that pass the provided filter
    /// criteria. Then refreshes this observable collection with the filtered items. Since refresh pauses change
    /// notification, this will update the collection without registering changes.
    /// </summary>
    /// <param name="filter">The filter function to apply to each item in the underlying collection.</param>
    public void Filter(Func<TObserver, bool> filter)
    {
        var collection = _refresh.Invoke();
        var filtered = collection.Where(filter).ToList();
        RefreshCollection(filtered);
    }

    /// <summary>
    /// Filters the ovserver collection based on the provided text by calling each observer's Filter method.
    /// </summary>
    /// <param name="filter">The text input used to filter the observers.</param>
    public void Filter(string? filter)
    {
        var collection = _refresh.Invoke().Where(x => x.Filter(filter)).ToList();
        RefreshCollection(collection);
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

    /// <summary>
    /// Disposes the ObserverCollection by calling Dispose method on each observer in the collection.
    /// It also suppresses finalization of the ObserverCollection instance.
    /// </summary>
    public void Dispose()
    {
        foreach (var observer in this)
        {
            observer.Dispose();
        }
    }

    /// <inheritdoc />
    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnCollectionChanged(e);

        //Don't notify change when refreshing.
        if (_refreshing) return;

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
        if (_refreshing) return;

        if (index == Count - 1)
        {
            _add?.Invoke(index, observer.Model);
        }
        else
        {
            _insert?.Invoke(index, observer.Model);
        }

        OnPropertyChanged(new PropertyChangedEventArgs(nameof(HasItems)));
    }

    /// <inheritdoc />
    protected override void RemoveItem(int index)
    {
        var observer = this[index];
        base.RemoveItem(index);
        Detach(observer);

        //Abort if refreshing to avoid circular calls.
        if (_refreshing) return;

        _remove?.Invoke(index, observer.Model);

        OnPropertyChanged(new PropertyChangedEventArgs(nameof(HasItems)));
    }

    protected override void MoveItem(int oldIndex, int newIndex)
    {
        base.MoveItem(oldIndex, newIndex);

        //Abort if refreshing to avoid circular calls.
        if (_refreshing) return;

        _move?.Invoke(oldIndex, newIndex);
    }

    /// <summary>Removes all items from the collection.</summary>
    protected override void ClearItems()
    {
        base.ClearItems();
        Detach(this);

        //Abort if refreshing to avoid circular calls.
        if (_refreshing) return;

        _clear?.Invoke();

        OnPropertyChanged(new PropertyChangedEventArgs(nameof(HasItems)));
    }

    /// <summary>
    /// Creates the initial observer collection to be passed up to the base class using the provided wrapper function.
    /// </summary>
    private static IEnumerable<TObserver> Initialize(ICollection<TModel> models, Func<TModel, TObserver> wrapper)
    {
        ArgumentNullException.ThrowIfNull(models);
        ArgumentNullException.ThrowIfNull(wrapper);
        return models.Select(wrapper);
    }

    /// <summary>
    /// Refreshes the underlying observable collection with the provided collection.
    /// </summary>
    /// <param name="collection">The collection to repalce the underlying collection with.</param>
    private void RefreshCollection(IEnumerable<TObserver> collection)
    {
        _refreshing = true;

        ClearItems();

        foreach (var observer in collection)
        {
            Add(observer);
        }

        _refreshing = false;
        _changed = false;
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsChanged)));
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(HasItems)));
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
/// Some helpers for the base <see cref="ObservableCollection{T}"/> class to make adding and refreshing its items easier.
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

    public static void Move<T>(this IList<T> list, int oldIndex, int newIndex)
    {
        var item = list[oldIndex];
        list.RemoveAt(oldIndex);
        list.Insert(newIndex, item);
    }

    public static ObserverCollection<TModel, TObserver> ToObserver<TModel, TObserver>(this IList<TModel> list,
        Func<TModel, TObserver> wrapper) where TObserver : Observer<TModel>
    {
        return new ObserverCollection<TModel, TObserver>(list, wrapper);
    }
}