using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using AutoSpex.Client.Observers;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using DynamicData;

namespace AutoSpex.Client.Components;

public class BreadcrumbMenu : TemplatedControl
{
    private ObservableCollection<Breadcrumb> _breadcrumbsSource = [];
    private readonly SourceList<Breadcrumb> _source = new();
    private readonly ReadOnlyObservableCollection<Breadcrumb> _breadcrumbs;
    private TextBox? _filterText;

    public BreadcrumbMenu()
    {
        _source.Connect().Bind(out _breadcrumbs).Subscribe();
        
        BreadcrumbsSourceProperty.Changed.AddClassHandler<BreadcrumbMenu>(
            (c, a) => c.SourceCollectionPropertyChanged(a));
    }

    public static readonly DirectProperty<BreadcrumbMenu, ObservableCollection<Breadcrumb>> BreadcrumbsSourceProperty =
        AvaloniaProperty.RegisterDirect<BreadcrumbMenu, ObservableCollection<Breadcrumb>>(
            nameof(BreadcrumbsSource), o => o.BreadcrumbsSource, (o, v) => o.BreadcrumbsSource = v);


    public ObservableCollection<Breadcrumb> BreadcrumbsSource
    {
        get => _breadcrumbsSource;
        set => SetAndRaise(BreadcrumbsSourceProperty, ref _breadcrumbsSource, value);
    }

    public ReadOnlyObservableCollection<Breadcrumb> Breadcrumbs => _breadcrumbs;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        RegisterFilterTextChange(e);
    }

    private void RegisterFilterTextChange(TemplateAppliedEventArgs e)
    {
        if (_filterText is not null)
            _filterText.TextChanged -= OnFilterTextChanged;

        _filterText = e.NameScope.Find("PART_FilterText") as TextBox;

        if (_filterText is not null)
            _filterText.TextChanged += OnFilterTextChanged;
    }

    private void OnFilterTextChanged(object? sender, TextChangedEventArgs? args)
    {
        if (args?.Source is not TextBox textBox) return;

        var filter = textBox.Text;

        _source.Edit(innerList =>
        {
            innerList.Clear();

            var filtered = string.IsNullOrEmpty(filter)
                ? BreadcrumbsSource
                : BreadcrumbsSource.Where(b => b.Model.Name.Contains(filter));

            innerList.AddRange(filtered);
        });
    }

    private void SourceCollectionPropertyChanged(AvaloniaPropertyChangedEventArgs args)
    {
        if (args.OldValue is ObservableCollection<Breadcrumb> old)
        {
            old.CollectionChanged -= SourceCollectionChanged;
        }

        if (args.NewValue is not ObservableCollection<Breadcrumb> pages) return;

        pages.CollectionChanged += SourceCollectionChanged;
        _source.Clear();
        _source.AddRange(pages);
    }

    private void SourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e is {Action: NotifyCollectionChangedAction.Add, NewItems: not null})
        {
            _source.AddRange(e.NewItems.OfType<Breadcrumb>());
            return;
        }

        _source.Clear();
        _source.AddRange(BreadcrumbsSource);
    }
}