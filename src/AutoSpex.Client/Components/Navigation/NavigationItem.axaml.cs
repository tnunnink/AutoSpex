using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Styling;

namespace AutoSpex.Client.Components;

public class NavigationItem : ContentControl
{
    private const string PartNameEntry = "NameEntry";
    private string? _itemName;
    private ICommand? _renameCommand;
    private ICommand? _navigateCommand;
    private TextBox? _nameEntry;

    #region AvaloniaProperties

    public static readonly StyledProperty<ControlTheme> IconThemeProperty =
        AvaloniaProperty.Register<NavigationItem, ControlTheme>(
            nameof(IconTheme));

    public static readonly DirectProperty<NavigationItem, string?> ItemNameProperty =
        AvaloniaProperty.RegisterDirect<NavigationItem, string?>(
            nameof(ItemName), o => o.ItemName, (o, v) => o.ItemName = v);

    public static readonly StyledProperty<bool> IsEditingProperty =
        AvaloniaProperty.Register<NavigationItem, bool>(
            nameof(IsEditing));

    public static readonly DirectProperty<NavigationItem, ICommand?> NavigateCommandProperty =
        AvaloniaProperty.RegisterDirect<NavigationItem, ICommand?>(
            nameof(NavigateCommand), o => o.NavigateCommand, (o, v) => o.NavigateCommand = v);

    public static readonly DirectProperty<NavigationItem, ICommand?> RenameCommandProperty =
        AvaloniaProperty.RegisterDirect<NavigationItem, ICommand?>(
            nameof(RenameCommand), o => o.RenameCommand, (o, v) => o.RenameCommand = v);

    #endregion

    static NavigationItem()
    {
        PointerPressedEvent.AddClassHandler<NavigationItem>((x, e) => x.PointerPressedHandler(e),
            RoutingStrategies.Tunnel);
    }

    public ControlTheme IconTheme
    {
        get => GetValue(IconThemeProperty);
        set => SetValue(IconThemeProperty, value);
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

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        RegisterEntryTextBox(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsEditingProperty)
            UpdateEditingVisualState();
    }

    /*protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (e.Source is not Control control) return;

        var point = e.GetCurrentPoint(control);

        if (!point.Properties.IsLeftButtonPressed || e.ClickCount != 2) return;
        e.Handled = true;
        NavigateCommand?.Execute(null);

        base.OnPointerPressed(e);
    }*/
    
    private void PointerPressedHandler(PointerPressedEventArgs e)
    {
        if (e.Source is not Control control) return;
        
        var point = e.GetCurrentPoint(control);
        if (!point.Properties.IsLeftButtonPressed || e.ClickCount != 2) return;
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
        _nameEntry?.Focus();
        e.Handled = true;
    }

    private void HandleEscapeKey(RoutedEventArgs e)
    {
        IsEditing = false;
        e.Handled = true;
    }

    private void HandleEnterKey(RoutedEventArgs e)
    {
        Console.WriteLine("Item Enter");
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
        _nameEntry.Focus();
    }
}