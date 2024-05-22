using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace AutoSpex.Client.Components;

public class BreadcrumbMenu : TemplatedControl
{
    #region Properties

    public static readonly DirectProperty<BreadcrumbMenu, ObservableCollection<NodeObserver>?> NodesProperty =
        AvaloniaProperty.RegisterDirect<BreadcrumbMenu, ObservableCollection<NodeObserver>?>(
            nameof(Nodes), o => o.Nodes, (o, v) => o.Nodes = v);

    public static readonly DirectProperty<BreadcrumbMenu, string?> FilterTextProperty =
        AvaloniaProperty.RegisterDirect<BreadcrumbMenu, string?>(
            nameof(FilterText), o => o.FilterText, (o, v) => o.FilterText = v);

    #endregion


    private ObservableCollection<NodeObserver>? _nodes = [];
    private string? _filterText;


    public ObservableCollection<NodeObserver>? Nodes
    {
        get => _nodes;
        set => SetAndRaise(NodesProperty, ref _nodes, value);
    }

    public string? FilterText
    {
        get => _filterText;
        set => SetAndRaise(FilterTextProperty, ref _filterText, value);
    }

    public ObservableCollection<NodeObserver> NodeCollection { get; } = [];

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        UpdateCollection();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == FilterTextProperty)
            UpdateCollection(change.GetNewValue<string?>());

        if (change.Property == NodesProperty)
            RegisterNodes(change);
    }

    private void RegisterNodes(AvaloniaPropertyChangedEventArgs args)
    {
        if (args.OldValue is ObservableCollection<NodeObserver> old)
        {
            old.CollectionChanged -= NodesCollectionChanged;
        }

        if (args.NewValue is not ObservableCollection<NodeObserver> pages) return;

        pages.CollectionChanged += NodesCollectionChanged;
        UpdateCollection();
    }

    private void NodesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e is { Action: NotifyCollectionChangedAction.Add, NewItems: not null })
        {
            NodeCollection.AddRange(e.NewItems.OfType<NodeObserver>());
            return;
        }

        UpdateCollection();
    }

    private void UpdateCollection(string? filter = default)
    {
        var filtered = Nodes?.Where(n => FilterNode(n, filter)) ?? Enumerable.Empty<NodeObserver>();
        NodeCollection.Refresh(filtered);
    }

    private static bool FilterNode(NodeObserver node, string? filter)
    {
        return string.IsNullOrEmpty(filter) || node.Name.Contains(filter, StringComparison.OrdinalIgnoreCase);
    }
}