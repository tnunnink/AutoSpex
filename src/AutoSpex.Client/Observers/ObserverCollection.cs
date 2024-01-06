using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace AutoSpex.Client.Observers;

public class ObserverCollection<TModel, TObserver>
    : ObservableCollection<TObserver>, IChangeTracking where TObserver : Observer<TModel>
{
    private int _changed;
    private readonly Action<int, TModel> _add;
    private readonly Action<int, TModel> _remove;
    private readonly Action? _clear;

    public ObserverCollection(Action<int, TModel> add, Action<int, TModel> remove, Action? clear = default)
    {
        _add = add ?? throw new ArgumentNullException(nameof(add));
        _remove = remove ?? throw new ArgumentNullException(nameof(remove));
        _clear = clear;

        Attach(this);
    }

    public ObserverCollection(IEnumerable<TObserver> collection,
        Action<int, TModel> add,
        Action<int, TModel> remove,
        Action? clear = default) : base(collection.ToList())
    {
        _add = add ?? throw new ArgumentNullException(nameof(add));
        _remove = remove ?? throw new ArgumentNullException(nameof(remove));
        _clear = clear;

        Attach(this);
    }

    public ObserverCollection(IList<TModel> models, Func<TModel, TObserver> wrapper)
        : base(Initialize(models, wrapper))
    {
        _add = models.Insert;
        _remove = (i, _) => models.RemoveAt(i);
        _clear = models.Clear;

        Attach(this);
    }

    public bool IsChanged => _changed > 0 || this.Any(o => o.IsChanged);

    public void AcceptChanges()
    {
        _changed = 0;

        foreach (var item in this)
        {
            item.AcceptChanges();
        }
    }

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnCollectionChanged(e);
        _changed++;
    }

    protected override void InsertItem(int index, TObserver observer)
    {
        base.InsertItem(index, observer);
        _add(index, observer.Model);
        Attach(observer);
    }

    protected override void RemoveItem(int index)
    {
        base.RemoveItem(index);
        var observer = this[index];
        _remove.Invoke(index, observer.Model);
        Detach(observer);
    }

    protected override void ClearItems()
    {
        base.ClearItems();
        _clear?.Invoke();
        Detach(this);
    }

    private static IEnumerable<TObserver> Initialize(IList<TModel> models, Func<TModel, TObserver> wrapper)
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