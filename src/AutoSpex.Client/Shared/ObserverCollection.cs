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

    public void Refresh(IEnumerable<TObserver>? observers = default)
    {
        IsRefreshing = true;
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsRefreshing)));

        ClearItems();

        var items = observers ?? _refresh();
        foreach (var item in items)
        {
            Add(item);
        }

        foreach (var observer in this)
        {
            observer.Refresh();
        }

        _changed = false;
        IsRefreshing = false;
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsRefreshing)));
    }

    public void Sort<TField>(Func<TObserver, TField> selector, IComparer<TField>? comparer)
    {
        var sorted = this.OrderBy(selector, comparer).ToList();

        for (var i = 0; i < sorted.Count; i++)
            Move(IndexOf(sorted[i]), i);
    }

    public IEnumerable GetErrors(string? propertyName)
    {
        return this.SelectMany(o => o.GetErrors(propertyName));
    }

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnCollectionChanged(e);

        if (IsRefreshing) return;
        _changed = true;
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsChanged)));
    }

    protected override void InsertItem(int index, TObserver observer)
    {
        base.InsertItem(index, observer);
        Attach(observer);

        if (IsRefreshing) return;

        if (index == Count - 1)
        {
            _add?.Invoke(index, observer.Model);
            return;
        }

        _insert?.Invoke(index, observer.Model);
    }

    protected override void RemoveItem(int index)
    {
        var observer = this[index];
        base.RemoveItem(index);
        Detach(observer);

        if (IsRefreshing) return;
        _remove?.Invoke(index, observer.Model);
    }

    protected override void ClearItems()
    {
        base.ClearItems();
        Detach(this);

        if (IsRefreshing) return;
        _clear?.Invoke();
    }

    private static IEnumerable<TObserver> Initialize(ICollection<TModel> models, Func<TModel, TObserver> wrapper)
    {
        if (models is null) throw new ArgumentNullException(nameof(models));
        if (wrapper is null) throw new ArgumentNullException(nameof(wrapper));
        return models.Select(wrapper);
    }

    private void Attach(IEnumerable<TObserver> observers)
    {
        foreach (var observer in observers)
            Attach(observer);
    }

    private void Attach(TObserver observer)
    {
        observer.PropertyChanged -= OnObserverPropertyChanged;
        observer.PropertyChanged += OnObserverPropertyChanged;

        observer.ErrorsChanged -= OnObserverErrorsChanged;
        observer.ErrorsChanged += OnObserverErrorsChanged;
    }

    private void Detach(IEnumerable<TObserver> observers)
    {
        foreach (var observer in observers)
            Detach(observer);
    }

    private void Detach(TObserver observer)
    {
        observer.PropertyChanged -= OnObserverPropertyChanged;
        observer.ErrorsChanged -= OnObserverErrorsChanged;
    }

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

    private void OnObserverErrorsChanged(object? sender, DataErrorsChangedEventArgs e)
    {
        if (sender is not TObserver observer) return;
        
        //Raise that an item property changed.
        ErrorsChanged?.Invoke(observer, e);
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(HasErrors)));
    }
}

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