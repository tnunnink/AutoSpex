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
    private bool _changed;
    private readonly Action<int, TModel>? _add;
    private readonly Action<int, TModel>? _remove;
    private readonly Action? _clear;

    public ObserverCollection(IList<TModel> models, Func<TModel, TObserver> wrapper,
        Action<int, TModel>? add = default,
        Action<int, TModel>? remove = default,
        Action? clear = default) : base(Initialize(models, wrapper))
    {
        _add = add ?? models.Insert;
        _remove = remove ?? ((i, _) => models.RemoveAt(i)); 
        _clear = clear ?? models.Clear;

        Attach(this);
    }

    public bool IsChanged => _changed || this.Any(o => o.IsChanged);

    public void AcceptChanges()
    {
        _changed = false;

        foreach (var item in this)
        {
            item.AcceptChanges();
        }
    }

    public void Refresh()
    {
        _changed = false;
    }

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnCollectionChanged(e);
        _changed = true;
    }

    protected override void InsertItem(int index, TObserver observer)
    {
        base.InsertItem(index, observer);
        _add?.Invoke(index, observer.Model);
        Attach(observer);
    }

    protected override void RemoveItem(int index)
    {
        base.RemoveItem(index);
        var observer = this[index];
        _remove?.Invoke(index, observer.Model);
        Detach(observer);
    }

    protected override void ClearItems()
    {
        base.ClearItems();
        _clear?.Invoke();
        Detach(this);
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