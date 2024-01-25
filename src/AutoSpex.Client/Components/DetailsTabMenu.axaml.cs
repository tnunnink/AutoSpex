using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;
using AutoSpex.Client.Shared;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using DynamicData;
using JetBrains.Annotations;

namespace AutoSpex.Client.Components;

[PublicAPI]
[TemplatePart("PART_FilterText", typeof(TextBox))]
public class DetailsTabMenu : TemplatedControl
{
    private readonly SourceList<PageViewModel> _source = new();
    private readonly ReadOnlyObservableCollection<PageViewModel> _openTabs;
    private ObservableCollection<PageViewModel> _openTabsSource = [];
    private ICommand? _closeTabCommand;
    private TextBox? _filterTextBox;

    public DetailsTabMenu()
    {
        _source.Connect().Bind(out _openTabs).Subscribe();
        OpenTabsSourceProperty.Changed.AddClassHandler<DetailsTabMenu>((c, a) => c.OnOpenTabsSourcePropertyChanged(a));
    }

    public static readonly DirectProperty<DetailsTabMenu, ObservableCollection<PageViewModel>> OpenTabsSourceProperty =
        AvaloniaProperty.RegisterDirect<DetailsTabMenu, ObservableCollection<PageViewModel>>(
            nameof(OpenTabsSource), o => o.OpenTabsSource, (o, v) => o.OpenTabsSource = v);

    public static readonly DirectProperty<DetailsTabMenu, ICommand?> CloseTabCommandProperty =
        AvaloniaProperty.RegisterDirect<DetailsTabMenu, ICommand?>(
            nameof(CloseTabCommand), o => o.CloseTabCommand, (o, v) => o.CloseTabCommand = v);
    
    public ReadOnlyObservableCollection<PageViewModel> OpenTabs => _openTabs;

    public ObservableCollection<PageViewModel> OpenTabsSource
    {
        get => _openTabsSource;
        set => SetAndRaise(OpenTabsSourceProperty, ref _openTabsSource, value);
    }
    
    public ICommand? CloseTabCommand
    {
        get => _closeTabCommand;
        set => SetAndRaise(CloseTabCommandProperty, ref _closeTabCommand, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        RegisterFilterTextChange(e);
    }

    private void RegisterFilterTextChange(TemplateAppliedEventArgs e)
    {
        if (_filterTextBox is not null)
            _filterTextBox.TextChanged -= OnFilterTextChanged;

        _filterTextBox = e.NameScope.Find("PART_FilterText") as TextBox;

        if (_filterTextBox is not null)
            _filterTextBox.TextChanged += OnFilterTextChanged;
    }

    private void OnFilterTextChanged(object? sender, TextChangedEventArgs? args)
    {
        var value = _filterTextBox?.Text;

        _source.Edit(innerList =>
        {
            innerList.Clear();

            var filtered = string.IsNullOrEmpty(value)
                ? OpenTabsSource
                : OpenTabsSource.Where(p => p.Title.Contains(value));

            innerList.AddRange(filtered);
        });
    }

    private void OnOpenTabsSourcePropertyChanged(AvaloniaPropertyChangedEventArgs args)
    {
        if (args.OldValue is ObservableCollection<PageViewModel> old)
        {
            old.CollectionChanged -= OpenTabsSourceCollectionChanged;
        }
        
        if (args.NewValue is not ObservableCollection<PageViewModel> pages) return;
        
        pages.CollectionChanged += OpenTabsSourceCollectionChanged;
        _source.Clear();
        _source.AddRange(pages);
    }
    
    private void OpenTabsSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e is {Action: NotifyCollectionChangedAction.Add, NewItems: not null})
        {
            _source.AddRange(e.NewItems.OfType<PageViewModel>());
            return;
        }

        _source.Clear();
        _source.AddRange(OpenTabsSource);
    }
}