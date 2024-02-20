using System;
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

    public void AcceptChanges()
    {
        _changed = false;

        foreach (var item in this)
        {
            item.AcceptChanges();
        }

        OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsChanged)));
    }

    public void Refresh()
    {
        IsRefreshing = true;
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsRefreshing)));

        ClearItems();

        var items = _refresh();
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

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnCollectionChanged(e);
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
        observer.PropertyChanged -= ItemPropertyChanged;
        observer.PropertyChanged += ItemPropertyChanged;
    }

    private void Detach(IEnumerable<TObserver> observers)
    {
        foreach (var observer in observers)
            Detach(observer);
    }

    private void Detach(TObserver observer)
    {
        observer.PropertyChanged -= ItemPropertyChanged;
    }

    private void ItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not TObserver || e.PropertyName != nameof(IsChanged)) return;
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsChanged)));
    }
}