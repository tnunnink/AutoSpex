using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Input;

namespace AutoSpex.Client.Components;

public class NavigationTree : TreeView
{
    #region Properties

    public static readonly DirectProperty<NavigationTree, bool> IsSearchActiveProperty =
        AvaloniaProperty.RegisterDirect<NavigationTree, bool>(
            nameof(IsSearchActive), o => o.IsSearchActive, (o, v) => o.IsSearchActive = v,
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly DirectProperty<NavigationTree, ICommand?> AddCommandProperty =
        AvaloniaProperty.RegisterDirect<NavigationTree, ICommand?>(
            nameof(AddCommand), o => o.AddCommand, (o, v) => o.AddCommand = v,
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly DirectProperty<NavigationTree, string?> AddToolTipProperty =
        AvaloniaProperty.RegisterDirect<NavigationTree, string?>(
            nameof(AddToolTip), o => o.AddToolTip, (o, v) => o.AddToolTip = v);

    #endregion

    private const string SearchTextPart = "SearchTextBox";
    private bool _isSearchActive;
    private ICommand? _addCommand;
    private string? _addToolTip;
    private TextBox? _searchText;

    public NavigationTree()
    {
        ToggleSearchCommand = new RelayCommand(ToggleSearch);
        ExpandAllCommand = new RelayCommand(ExpandAll);
        CollapseAllCommand = new RelayCommand(CollapseAll);
    }

    public bool IsSearchActive
    {
        get => _isSearchActive;
        set => SetAndRaise(IsSearchActiveProperty, ref _isSearchActive, value);
    }

    public ICommand? AddCommand
    {
        get => _addCommand;
        set => SetAndRaise(AddCommandProperty, ref _addCommand, value);
    }

    public string? AddToolTip
    {
        get => _addToolTip;
        set => SetAndRaise(AddToolTipProperty, ref _addToolTip, value);
    }

    public ICommand ToggleSearchCommand { get; private set; }
    public ICommand ExpandAllCommand { get; private set; }
    public ICommand CollapseAllCommand { get; private set; }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new NavigationTreeItem();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e is {Key: Key.F, KeyModifiers: KeyModifiers.Control})
            HandleSearchKeyDown(e);

        //This will prevent the enter button from collapsing and expanding the node.
        //I don't want that behavior. I want then enter button to open the node.
        if (e is {Source: TreeViewItem, Key: Key.Enter}) e.Handled = true;

        base.OnKeyDown(e);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        RegisterFilterTextChange(e);
    }

    private void RegisterFilterTextChange(TemplateAppliedEventArgs e)
    {
        if (_searchText is not null) _searchText.TextChanged -= OnSearchTextChanged;
        _searchText = e.NameScope.Find(SearchTextPart) as TextBox;
        if (_searchText is not null) _searchText.TextChanged += OnSearchTextChanged;
    }

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs? args)
    {
        if (args?.Source is not TextBox textBox) return;

        foreach (var container in GetRealizedTreeContainers())
        {
            if (container is not NavigationTreeItem item) return;
            item.FilterItem(textBox.Text);
        }
    }

    private void HandleSearchKeyDown(RoutedEventArgs e)
    {
        IsSearchActive = true;
        e.Handled = true;
    }

    private void ToggleSearch()
    {
        IsSearchActive = !IsSearchActive;
    }

    private void ExpandAll()
    {
        foreach (var container in GetRealizedTreeContainers())
        {
            if (container is not TreeViewItem item) return;
            ExpandSubTree(item);
        }
    }

    private void CollapseAll()
    {
        foreach (var container in GetRealizedTreeContainers())
        {
            if (container is not TreeViewItem item) return;
            CollapseSubTree(item);
        }
    }
}