using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using AutoSpex.Client.Observers;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;
using DynamicData;

namespace AutoSpex.Client.Components;

public class SourceList : TemplatedControl
{
    #region AvaloniaProperties

    public static readonly DirectProperty<SourceList, ObservableCollection<SourceObserver>> SourcesProperty =
        AvaloniaProperty.RegisterDirect<SourceList, ObservableCollection<SourceObserver>>(
            nameof(Sources), o => o.Sources, (o, v) => o.Sources = v);

    public static readonly DirectProperty<SourceList, ICommand?> AddSourceCommandProperty =
        AvaloniaProperty.RegisterDirect<SourceList, ICommand?>(
            nameof(AddSourceCommand), o => o.AddSourceCommand, (o, v) => o.AddSourceCommand = v);

    #endregion

    private ObservableCollection<SourceObserver> _sources = [];
    private ICommand? _addSourceCommand;
    private TextBox? _filterText;
    private ListBox? _sourceList;

    public ObservableCollection<SourceObserver> SourceCollection { get; } = [];

    public ObservableCollection<SourceObserver> Sources
    {
        get => _sources;
        set => SetAndRaise(SourcesProperty, ref _sources, value);
    }

    public ICommand? AddSourceCommand
    {
        get => _addSourceCommand;
        set => SetAndRaise(AddSourceCommandProperty, ref _addSourceCommand, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        RegisterSourceList(e);
        RegisterFilterText(e);
        UpdateSourceList();
    }

    private void RegisterSourceList(TemplateAppliedEventArgs e)
    {
        _sourceList?.RemoveHandler(GotFocusEvent, SourceListGotFocus);
        _sourceList = e.NameScope.Find<ListBox>("SourceList");
        _sourceList?.AddHandler(GotFocusEvent, SourceListGotFocus);
    }

    private static void SourceListGotFocus(object? sender, GotFocusEventArgs e)
    {
        if (e.Source is not ListBoxItem item) return;
        var navigationItem = item.FindDescendantOfType<NavigationItem>();
        navigationItem?.Focus();
    }

    private void RegisterFilterText(TemplateAppliedEventArgs e)
    {
        if (_filterText is not null) _filterText.TextChanged -= FilterTextChanged;
        _filterText = e.NameScope.Find<TextBox>("FilterText");
        if (_filterText is not null) _filterText.TextChanged += FilterTextChanged;
    }

    private void FilterTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (e.Source is not TextBox textBox) return;
        var filter = textBox.Text ?? string.Empty;
        UpdateSourceList(filter);
    }

    private void UpdateSourceList(string? filter = default)
    {
        var filtered = Sources.Where(s => FilterSource(s, filter));
        SourceCollection.Clear();
        SourceCollection.AddRange(filtered);
    }

    private static bool FilterSource(SourceObserver source, string? text)
    {
        if (string.IsNullOrEmpty(text)) return true;
        if (source.Name.Contains(text, StringComparison.OrdinalIgnoreCase)) return true;
        if (source.TargetType.Contains(text, StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }
}