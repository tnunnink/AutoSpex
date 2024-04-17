using System;
using System.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Styling;
using JetBrains.Annotations;

namespace AutoSpex.Client.Components;

public class NavigationTreeItem : TreeViewItem
{
    private const string PartNameEntry = "NameEntry";
    private string? _itemName;
    private ICommand? _renameCommand;
    private ICommand? _navigateCommand;
    private TextBox? _nameEntry;

    #region AvaloniaProperties

    [PublicAPI] public static readonly StyledProperty<ControlTheme> IconThemeProperty =
        AvaloniaProperty.Register<NavigationTreeItem, ControlTheme>(nameof(IconTheme));

    [PublicAPI] public static readonly StyledProperty<double> ItemIndentProperty =
        AvaloniaProperty.Register<NavigationTreeItem, double>(
            nameof(ItemIndent), defaultValue: 22.0);

    [PublicAPI] public static readonly DirectProperty<NavigationTreeItem, string?> ItemNameProperty =
        AvaloniaProperty.RegisterDirect<NavigationTreeItem, string?>(
            nameof(ItemName), o => o.ItemName, (o, v) => o.ItemName = v);

    [PublicAPI] public static readonly StyledProperty<bool> IsEditingProperty =
        AvaloniaProperty.Register<NavigationTreeItem, bool>(
            nameof(IsEditing), defaultBindingMode: BindingMode.TwoWay);

    [PublicAPI] public static readonly StyledProperty<bool> IsSearchMatchProperty =
        AvaloniaProperty.Register<NavigationTreeItem, bool>(
            nameof(IsSearchMatch));

    [PublicAPI] public static readonly DirectProperty<NavigationTreeItem, ICommand?> NavigateCommandProperty =
        AvaloniaProperty.RegisterDirect<NavigationTreeItem, ICommand?>(
            nameof(NavigateCommand), o => o.NavigateCommand, (o, v) => o.NavigateCommand = v);

    [PublicAPI] public static readonly DirectProperty<NavigationTreeItem, ICommand?> RenameCommandProperty =
        AvaloniaProperty.RegisterDirect<NavigationTreeItem, ICommand?>(
            nameof(RenameCommand), o => o.RenameCommand, (o, v) => o.RenameCommand = v);

    #endregion

    public ControlTheme IconTheme
    {
        get => GetValue(IconThemeProperty);
        set => SetValue(IconThemeProperty, value);
    }

    public double ItemIndent
    {
        get => GetValue(ItemIndentProperty);
        set => SetValue(ItemIndentProperty, value);
    }

    public string? ItemName
    {
        get => _itemName;
        set => SetAndRaise(ItemNameProperty, ref _itemName, value);
    }

    public bool IsEditing
    {
        get => GetValue(IsEditingProperty);
        set => SetValue(IsEditingProperty, value);
    }

    public bool IsSearchMatch
    {
        get => GetValue(IsSearchMatchProperty);
        set => SetValue(IsSearchMatchProperty, value);
    }

    public ICommand? NavigateCommand
    {
        get => _navigateCommand;
        set => SetAndRaise(NavigateCommandProperty, ref _navigateCommand, value);
    }

    public ICommand? RenameCommand
    {
        get => _renameCommand;
        set => SetAndRaise(RenameCommandProperty, ref _renameCommand, value);
    }

    public bool FilterItem(string? text)
    {
        var children = LogicalChildren.OfType<NavigationTreeItem>().Select(x => x.FilterItem(text)).Count(c => c);

        if (string.IsNullOrEmpty(text))
        {
            IsVisible = true;
            IsExpanded = false;
            IsSearchMatch = false;
        }
        else
        {
            IsVisible = ItemName?.Contains(text, StringComparison.OrdinalIgnoreCase) is true || children > 0;
            IsExpanded = IsVisible && Items.Count > 0;
            IsSearchMatch = IsVisible;
        }

        return IsVisible;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        RegisterEntryTextBox(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        /*if (change.Property == IsSelectedProperty)
            SelectionStateChanged(change.GetNewValue<bool>());*/

        if (change.Property == IsEditingProperty)
            UpdateEditingVisualState();
    }

    /*private void SelectionStateChanged(bool selected)
    {
        if (!selected)
            IsEditing = false;
    }*/

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (e.Source is not Control control) return;

        var point = e.GetCurrentPoint(control);

        if (!point.Properties.IsLeftButtonPressed || e.ClickCount != 2)
        {
            base.OnPointerPressed(e);
            return;
        }

        e.Handled = true;
        NavigateCommand?.Execute(null);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e is {Key: Key.E, KeyModifiers: KeyModifiers.Control}) HandleEditKey(e);
        if (e is {Key: Key.Escape}) HandleEscapeKey(e);
        if (e is {Key: Key.Enter}) HandleEnterKey(e);
        base.OnKeyDown(e);
    }

    private void HandleEditKey(RoutedEventArgs e)
    {
        IsEditing = true;
        e.Handled = true;
    }

    private void HandleEscapeKey(RoutedEventArgs e)
    {
        IsEditing = false;
        e.Handled = true;
    }

    private void HandleEnterKey(RoutedEventArgs e)
    {
        if (e.Source is TextBox textBox && IsEditing)
        {
            RenameCommand?.Execute(textBox.Text);
            IsEditing = false;
            e.Handled = true;
            return;
        }

        NavigateCommand?.Execute(null);
        e.Handled = true;
    }

    private void RegisterEntryTextBox(TemplateAppliedEventArgs args)
    {
        _nameEntry?.RemoveHandler(LostFocusEvent, EntryLostFocus);
        _nameEntry = args.NameScope.Find<TextBox>(PartNameEntry);
        _nameEntry?.AddHandler(LostFocusEvent, EntryLostFocus, RoutingStrategies.Bubble);
    }

    private void EntryLostFocus(object? sender, RoutedEventArgs e)
    {
        IsEditing = false;
        e.Handled = true;
    }

    private void UpdateEditingVisualState()
    {
        if (_nameEntry is null || !IsEditing)
        {
            Focus();
            return;
        }

        _nameEntry.Text = ItemName;
        _nameEntry.SelectAll();
    }
}