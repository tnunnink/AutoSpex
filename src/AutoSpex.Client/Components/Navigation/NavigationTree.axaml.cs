using System.Windows.Input;
using AutoSpex.Client.Resources.Controls;
using AutoSpex.Engine;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using CommunityToolkit.Mvvm.Input;

namespace AutoSpex.Client.Components;

public class NavigationTree : TreeView
{
    #region Properties

    public static readonly DirectProperty<NavigationTree, NodeType?> FeatureProperty =
        AvaloniaProperty.RegisterDirect<NavigationTree, NodeType?>(
            nameof(Feature), o => o.Feature, (o, v) => o.Feature = v,
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly DirectProperty<NavigationTree, bool> IsSearchActiveProperty =
        AvaloniaProperty.RegisterDirect<NavigationTree, bool>(
            nameof(IsSearchActive), o => o.IsSearchActive, (o, v) => o.IsSearchActive = v,
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly DirectProperty<NavigationTree, ICommand?> AddItemCommandProperty =
        AvaloniaProperty.RegisterDirect<NavigationTree, ICommand?>(
            nameof(AddItemCommand), o => o.AddItemCommand, (o, v) => o.AddItemCommand = v,
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly DirectProperty<NavigationTree, ICommand?> AddContainerCommandProperty =
        AvaloniaProperty.RegisterDirect<NavigationTree, ICommand?>(
            nameof(AddContainerCommand), o => o.AddContainerCommand, (o, v) => o.AddContainerCommand = v,
            defaultBindingMode: BindingMode.TwoWay);

    #endregion

    private const string SearchTextPart = "SearchTextBox";
    private NodeType? _feature;
    private bool _isSearchActive;
    private ICommand? _addItemCommand;
    private ICommand? _addContainerCommand;
    private TextBox? _searchText;

    public NavigationTree()
    {
        ExpandAllCommand = new RelayCommand(ExpandAll);
        CollapseAllCommand = new RelayCommand(CollapseAll);
        HideCommand = new RelayCommand(HidePanel);
    }

    public NodeType? Feature
    {
        get => _feature;
        set => SetAndRaise(FeatureProperty, ref _feature, value);
    }

    public bool IsSearchActive
    {
        get => _isSearchActive;
        set => SetAndRaise(IsSearchActiveProperty, ref _isSearchActive, value);
    }

    public ICommand? AddItemCommand
    {
        get => _addItemCommand;
        set => SetAndRaise(AddItemCommandProperty, ref _addItemCommand, value);
    }

    public ICommand? AddContainerCommand
    {
        get => _addContainerCommand;
        set => SetAndRaise(AddContainerCommandProperty, ref _addContainerCommand, value);
    }

    public ICommand ExpandAllCommand { get; private set; }
    public ICommand CollapseAllCommand { get; private set; }
    public ICommand HideCommand { get; private set; }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new NavigationTreeItem();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e is { Key: Key.F, KeyModifiers: KeyModifiers.Control })
            HandleSearchKeyDown(e);

        //This will prevent the enter button from collapsing and expanding the node.
        //I don't want that behavior. I want then enter button to open the node.
        if (e is { Source: TreeViewItem, Key: Key.Enter }) e.Handled = true;

        base.OnKeyDown(e);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            e.Handled = true;
            return;
        }

        base.OnPointerPressed(e);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        RegisterFilterTextChange(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsSearchActiveProperty)
            HandleIsSearchActiveChanged(change.GetNewValue<bool>());
    }

    /// <summary>
    /// When filter is active apply the current search filter, otherwise show everything. 
    /// </summary>
    private void HandleIsSearchActiveChanged(bool value)
    {
        if (value)
        {
            ApplyFilter(_searchText?.Text ?? string.Empty);
            return;
        }

        ApplyFilter(string.Empty);
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
        ApplyFilter(textBox.Text ?? string.Empty);
    }

    private void ApplyFilter(string filter)
    {
        foreach (var container in GetRealizedTreeContainers())
        {
            if (container is not NavigationTreeItem item) return;
            item.FilterItem(filter);
        }
    }

    private void HandleSearchKeyDown(RoutedEventArgs e)
    {
        IsSearchActive = true;
        e.Handled = true;
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

    private void HidePanel()
    {
        var drawer = this.FindLogicalAncestorOfType<DrawerView>();
        if (drawer is null) return;
        drawer.IsDrawerOpen = false;
    }
}