using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using AutoSpex.Client.Observers;
using AutoSpex.Engine;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using JetBrains.Annotations;

namespace AutoSpex.Client.Components;

[PublicAPI]
public class SpecTreeView : TreeView
{
    private const string TreePart = "PART_TreeView";
    private const string FilterTextPart = "PART_FilterText";

    private ICommand? _addCommand;
    private ICommand? _openCommand;
    private TreeView? _treeView;
    private TextBox? _filterTextBox;

    public static readonly DirectProperty<SpecTreeView, ICommand?> AddCommandProperty =
        AvaloniaProperty.RegisterDirect<SpecTreeView, ICommand?>(
            nameof(AddCommand), o => o.AddCommand, (o, v) => o.AddCommand = v);

    public static readonly DirectProperty<SpecTreeView, ICommand?> OpenCommandProperty =
        AvaloniaProperty.RegisterDirect<SpecTreeView, ICommand?>(
            nameof(OpenCommand), o => o.OpenCommand, (o, v) => o.OpenCommand = v,
            defaultBindingMode: BindingMode.TwoWay);

    public ICommand? AddCommand
    {
        get => _addCommand;
        set => SetAndRaise(AddCommandProperty, ref _addCommand, value);
    }

    public ICommand? OpenCommand
    {
        get => _openCommand;
        set => SetAndRaise(OpenCommandProperty, ref _openCommand, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        RegisterTreeViewItemPointerRelease(e);
        RegisterFilterTextChange(e);
    }

    private void RegisterTreeViewItemPointerRelease(TemplateAppliedEventArgs args)
    {
        if (_treeView is not null) _treeView.PointerReleased -= OnTreePointerReleased;
        _treeView = args.NameScope.Find(TreePart) as TreeView;
        if (_treeView is not null) _treeView.PointerReleased += OnTreePointerReleased;
    }
    
    private void RegisterTreeViewItemPointerPressed(TemplateAppliedEventArgs args)
    {
        if (_treeView is not null) _treeView.PointerPressed -= OnTreePointerPressed;
        _treeView = args.NameScope.Find(TreePart) as TreeView;
        if (_treeView is not null) _treeView.PointerPressed += OnTreePointerPressed;
    }

    private void OnTreePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Source is not Control {DataContext: NodeObserver node}) return;

        if (e.ClickCount == 2)
        {
            OpenCommand?.Execute(node);
        }
    }

    private void RegisterFilterTextChange(TemplateAppliedEventArgs e)
    {
        if (_filterTextBox is not null) _filterTextBox.TextChanged -= OnFilterTextChanged;
        _filterTextBox = e.NameScope.Find(FilterTextPart) as TextBox;
        if (_filterTextBox is not null) _filterTextBox.TextChanged += OnFilterTextChanged;
    }

    private void OnTreePointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (e.Source is not Control {DataContext: NodeObserver node}) return;
        
        if (e.InitialPressMouseButton == MouseButton.Left)
        {
            OpenCommand?.Execute(node);
        }
    }

    private void OnFilterTextChanged(object? sender, TextChangedEventArgs? args)
    {
        if (args?.Source is not TextBox textBox || _treeView is null) return;
        
        var items = _treeView.GetRealizedTreeContainers().Cast<TreeViewItem>().ToList();
        
        foreach (var item in items)
        {
            FilterNode(item, textBox.Text);
        }
    }

    private bool FilterNode(TreeViewItem treeViewItem, string? filter)
    {
        if (treeViewItem.DataContext is not NodeObserver node) return false;

        var descendents = treeViewItem.GetRealizedContainers().Cast<TreeViewItem>()
            .Select(t => FilterNode(t, filter))
            .Where(r => r)
            .ToList().Count;

        if (string.IsNullOrEmpty(filter))
        {
            treeViewItem.IsVisible = true;
            treeViewItem.IsExpanded = false;
        }
        else
        {
            treeViewItem.IsVisible = node.Name.Contains(filter) || descendents > 0;
            treeViewItem.IsExpanded =
                IsVisible && (node.NodeType == NodeType.Folder || node.NodeType == NodeType.Collection);
        }

        return IsVisible;
    }
}

public class DesignNodes
{
    public static ObservableCollection<NodeObserver> Nodes = new(GenerateNodes().Select(n => new NodeObserver(n)));
    
    private static IEnumerable<Node> GenerateNodes()
    {
        var collection = Node.NewCollection();
        var folder = collection.AddFolder();
        collection.AddSpec();
        folder.AddSpec();
        yield return collection;
        yield return Node.NewCollection();
    }
}