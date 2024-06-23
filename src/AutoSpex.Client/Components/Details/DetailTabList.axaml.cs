using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using AutoSpex.Client.Shared;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace AutoSpex.Client.Components;

public class DetailTabList : TemplatedControl
{
    public static readonly DirectProperty<DetailTabList, ObservableCollection<DetailPageModel>?> TabsProperty =
        AvaloniaProperty.RegisterDirect<DetailTabList, ObservableCollection<DetailPageModel>?>(
            nameof(Tabs), o => o.Tabs, (o, v) => o.Tabs = v);

    public static readonly DirectProperty<DetailTabList, string?> FilterTextProperty =
        AvaloniaProperty.RegisterDirect<DetailTabList, string?>(
            nameof(FilterText), o => o.FilterText, (o, v) => o.FilterText = v);

    private ObservableCollection<DetailPageModel>? _tabs = [];
    private string? _filterText;

    public ObservableCollection<DetailPageModel>? Tabs
    {
        get => _tabs;
        set => SetAndRaise(TabsProperty, ref _tabs, value);
    }

    public string? FilterText
    {
        get => _filterText;
        set => SetAndRaise(FilterTextProperty, ref _filterText, value);
    }

    public ObservableCollection<DetailPageModel> TabCollection { get; } = [];

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        UpdateTabCollection();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == FilterTextProperty)
            UpdateTabCollection(change.GetNewValue<string?>());

        if (change.Property == TabsProperty)
            RegisterTabsCollection(change);
    }

    private void RegisterTabsCollection(AvaloniaPropertyChangedEventArgs args)
    {
        if (args.OldValue is ObservableCollection<DetailPageModel> old)
            old.CollectionChanged -= OpenTabsSourceCollectionChanged;

        if (args.NewValue is not ObservableCollection<DetailPageModel> tabs) return;

        tabs.CollectionChanged += OpenTabsSourceCollectionChanged;
        UpdateTabCollection();
    }

    private void OpenTabsSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e is { Action: NotifyCollectionChangedAction.Add, NewItems: not null })
        {
            TabCollection.AddRange(e.NewItems.OfType<DetailPageModel>());
            return;
        }

        UpdateTabCollection();
    }

    private void UpdateTabCollection(string? filter = default)
    {
        TabCollection.Clear();

        var collection = Tabs?.Where(t => FilterTab(t, filter)) ?? Enumerable.Empty<DetailPageModel>();

        foreach (var item in collection)
        {
            TabCollection.Add(item);
        }
    }

    private static bool FilterTab(PageViewModel page, string? filter)
    {
        return string.IsNullOrEmpty(filter) || page.Title.Contains(filter, StringComparison.OrdinalIgnoreCase);
    }
}