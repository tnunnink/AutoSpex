using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.LogicalTree;

namespace AutoSpex.Client.Components;

public class HomeMenu : TemplatedControl
{
    private const string MenuPart = "PART_Menu";
    private string _selectedMenu = string.Empty;
    private ListBox? _menu;

    public static readonly DirectProperty<HomeMenu, string> SelectedMenuProperty =
        AvaloniaProperty.RegisterDirect<HomeMenu, string>(
            nameof(SelectedMenu), o => o.SelectedMenu, (o, v) => o.SelectedMenu = v,
            defaultBindingMode: BindingMode.TwoWay);

    public string SelectedMenu
    {
        get => _selectedMenu;
        set => SetAndRaise(SelectedMenuProperty, ref _selectedMenu, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        AttacheMenuItemSelectionChangeHandler(e);
        InitializeSelectedItem();
    }

    private void InitializeSelectedItem()
    {
        if (_menu is null) return;
        _menu.SelectedItem ??= _menu.Items[0];
    }

    private void AttacheMenuItemSelectionChangeHandler(TemplateAppliedEventArgs e)
    {
        if (_menu is not null) _menu.SelectionChanged -= OnMenuSelectionChanged;
        _menu = e.NameScope.Find<ListBox>(MenuPart);
        if (_menu is not null) _menu.SelectionChanged += OnMenuSelectionChanged;
    }

    private void OnMenuSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count == 0) return;
        if (e.AddedItems[0] is not ListBoxItem item) return;
        var text = item.FindLogicalDescendantOfType<TextBlock>()?.Text;
        SelectedMenu = text ?? string.Empty;
    }
}