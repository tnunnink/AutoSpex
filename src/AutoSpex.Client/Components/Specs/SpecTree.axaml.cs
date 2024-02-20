using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using AutoSpex.Client.Observers;
using AutoSpex.Engine;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Input;

namespace AutoSpex.Client.Components;

public class SpecTree : TemplatedControl
{
    private const string TreePart = "PART_TreeView";
    private const string FilterTextPart = "PART_FilterText";

    #region Proeprties

    public static readonly DirectProperty<SpecTree, ObservableCollection<NodeObserver>> CollectionsProperty =
        AvaloniaProperty.RegisterDirect<SpecTree, ObservableCollection<NodeObserver>>(
            nameof(Collections), o => o.Collections, (o, v) => o.Collections = v);

    public static readonly DirectProperty<SpecTree, ICommand?> AddCommandProperty =
        AvaloniaProperty.RegisterDirect<SpecTree, ICommand?>(
            nameof(AddCommand), o => o.AddCommand, (o, v) => o.AddCommand = v);

    public static readonly DirectProperty<SpecTree, ICommand?> ExpandAllCommandProperty =
        AvaloniaProperty.RegisterDirect<SpecTree, ICommand?>(
            nameof(ExpandAllCommand), o => o.ExpandAllCommand, (o, v) => o.ExpandAllCommand = v);

    public static readonly DirectProperty<SpecTree, ICommand?> CollapseAllCommandProperty =
        AvaloniaProperty.RegisterDirect<SpecTree, ICommand?>(
            nameof(CollapseAllCommand), o => o.CollapseAllCommand, (o, v) => o.CollapseAllCommand = v);

    #endregion

    private TreeView? _treeView;
    private TextBox? _filterText;
    private ObservableCollection<NodeObserver> _collections = [];
    private ICommand? _addCommand;
    private ICommand? _expandAllCommand;
    private ICommand? _collapseAllCommand;

    static SpecTree()
    {
        KeyDownEvent.AddClassHandler<SpecTree>((_, args) => HandleKeyDownEvent(args), RoutingStrategies.Tunnel);
    }

    public SpecTree()
    {
        ExpandAllCommand = new RelayCommand(ExpandAll);
        CollapseAllCommand = new RelayCommand(CollapseAll);
    }

    public ObservableCollection<NodeObserver> Collections
    {
        get => _collections;
        set => SetAndRaise(CollectionsProperty, ref _collections, value);
    }

    public ICommand? AddCommand
    {
        get => _addCommand;
        set => SetAndRaise(AddCommandProperty, ref _addCommand, value);
    }

    public ICommand? ExpandAllCommand
    {
        get => _expandAllCommand;
        set => SetAndRaise(ExpandAllCommandProperty, ref _expandAllCommand, value);
    }

    public ICommand? CollapseAllCommand
    {
        get => _collapseAllCommand;
        set => SetAndRaise(CollapseAllCommandProperty, ref _collapseAllCommand, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        RegisterTreeViewEvents(e);
        RegisterFilterTextChange(e);
    }

    private void RegisterTreeViewEvents(TemplateAppliedEventArgs args)
    {
        if (_treeView is not null)
        {
            _treeView.PointerReleased -= TreeViewPointerReleased;
            //_treeView.PointerPressed -= TreeViewPointerPressed;
            _treeView.GotFocus -= TreeViewGotFocus;
            _treeView.LostFocus -= TreeViewLostFocus;
        }

        _treeView = args.NameScope.Find(TreePart) as TreeView;
        if (_treeView is null) return;

        _treeView.PointerReleased += TreeViewPointerReleased;
        //_treeView.PointerPressed += TreeViewPointerPressed;
        _treeView.GotFocus += TreeViewGotFocus;
        _treeView.LostFocus += TreeViewLostFocus;
    }

    private static async void TreeViewPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (e.Source is not Control {DataContext: NodeObserver node}) return;

        if (e.InitialPressMouseButton == MouseButton.Left)
        {
            await node.Navigate();
        }
    }

    /*private void TreeViewPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Source is not Control {DataContext: NodeObserver node}) return;

        if (e.ClickCount == 2)
        {
            node.OpenCommand.Execute(null);
        }
    }*/

    private static void HandleKeyDownEvent(KeyEventArgs args)
    {
        if (args is {Key: Key.E, KeyModifiers: KeyModifiers.Control}) TreeViewHandleEditKeys(args);
        if (args is {Key: Key.Escape}) TreeViewHandleEscapeKey(args);
        if (args is {Key: Key.Enter}) TreeViewHandleEnterKey(args);
    }

    private static void TreeViewGotFocus(object? sender, GotFocusEventArgs e)
    {
        if (e.Source is not TextBox {DataContext: NodeObserver} textBox) return;
        Dispatcher.UIThread.Post(() => textBox.SelectAll());
    }

    private static void TreeViewLostFocus(object? sender, RoutedEventArgs e)
    {
        if (e.Source is not TextBox {DataContext: NodeObserver node}) return;

        Dispatcher.UIThread.Post(() =>
        {
            node.IsEditing = false;
            node.Refresh();
        });
    }

    private static void TreeViewHandleEditKeys(RoutedEventArgs e)
    {
        if (e.Source is not TreeViewItem {DataContext: NodeObserver node} treeViewItem) return;
        node.IsEditing = true;
        var entry = treeViewItem.FindDescendantOfType<TextBox>();
        Dispatcher.UIThread.Post(() => { entry?.Focus(); });
    }

    private static void TreeViewHandleEscapeKey(RoutedEventArgs e)
    {
        if (e.Source is not TextBox textBox) return;
        var shell = textBox.FindAncestorOfType<Window>();
        Dispatcher.UIThread.Post(() => { shell?.Focus(); });
    }

    private static void TreeViewHandleEnterKey(RoutedEventArgs e)
    {
        e.Handled = true; //regardless set handled because I don't want enter to expand/collapse tree.

        if (e.Source is not TextBox {DataContext: NodeObserver node} textBox) return;

        node.RenameNodeCommand.ExecuteAsync(null);
        var shell = textBox.FindAncestorOfType<Window>();
        Dispatcher.UIThread.Post(() => { shell?.Focus(); });
    }

    private void RegisterFilterTextChange(TemplateAppliedEventArgs e)
    {
        if (_filterText is not null) _filterText.TextChanged -= OnFilterTextChanged;
        _filterText = e.NameScope.Find(FilterTextPart) as TextBox;
        if (_filterText is not null) _filterText.TextChanged += OnFilterTextChanged;
    }

    private void OnFilterTextChanged(object? sender, TextChangedEventArgs? args)
    {
        if (args?.Source is not TextBox textBox) return;

        foreach (var collection in Collections)
        {
            FilterNode(collection, textBox.Text);
        }
    }

    private static bool FilterNode(NodeObserver node, string? filter)
    {
        var passingChildren = node.Nodes.Select(n => FilterNode(n, filter)).Count(r => r);

        if (string.IsNullOrEmpty(filter))
        {
            node.IsVisible = true;
            node.IsExpanded = false;
        }
        else
        {
            node.IsVisible = node.Name.Contains(filter, StringComparison.OrdinalIgnoreCase) || passingChildren > 0;
            node.IsExpanded = node.IsVisible &&
                              (node.NodeType == NodeType.Folder || node.NodeType == NodeType.Collection);
        }

        return node.IsVisible;
    }
    
    private void ExpandAll()
    {
        var items = _treeView?.GetRealizedTreeContainers().Cast<TreeViewItem>() ?? Enumerable.Empty<TreeViewItem>();
        
        foreach (var item in items)
        {
            _treeView?.ExpandSubTree(item);
        }
    }
    
    private void CollapseAll()
    {
        var items = _treeView?.GetRealizedTreeContainers().Cast<TreeViewItem>() ?? Enumerable.Empty<TreeViewItem>();
        
        foreach (var item in items)
        {
            _treeView?.CollapseSubTree(item);
        }
    }
}