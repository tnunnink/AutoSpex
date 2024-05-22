using System;
using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Engine;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace AutoSpex.Client.Components;

[PseudoClasses(PcDragOver)]
public class NavigationTreeItem : TreeViewItem
{
    #region AvaloniaProperties

    public static readonly StyledProperty<bool> IsSearchMatchProperty =
        AvaloniaProperty.Register<NavigationTreeItem, bool>(
            nameof(IsSearchMatch));

    #endregion

    private const string PcDragOver = ":dragover";

    static NavigationTreeItem()
    {
        DragDrop.DragEnterEvent.AddClassHandler<NavigationTreeItem>((i, a) => i.OnDragEnter(a),
            RoutingStrategies.Bubble);
        DragDrop.DragLeaveEvent.AddClassHandler<NavigationTreeItem>((i, a) => i.OnDragLeave(a),
            RoutingStrategies.Bubble);
        DragDrop.DropEvent.AddClassHandler<NavigationTreeItem>((i, a) => i.OnDrop(a),
            RoutingStrategies.Bubble);
    }

    public bool IsSearchMatch
    {
        get => GetValue(IsSearchMatchProperty);
        set => SetValue(IsSearchMatchProperty, value);
    }

    public bool FilterItem(string? text)
    {
        var children = LogicalChildren.OfType<NavigationTreeItem>().Select(x => x.FilterItem(text)).Count(c => c);

        if (DataContext is not NodeObserver node) return false;

        if (string.IsNullOrEmpty(text))
        {
            IsVisible = true;
            IsExpanded = false;
        }
        else
        {
            IsVisible = node.Name.Contains(text, StringComparison.OrdinalIgnoreCase) || children > 0;
            IsExpanded = IsVisible && Items.Count > 0;
        }

        return IsVisible;
    }

    /// <summary>
    /// Overriding to navigate our nodes when the item is double-clicked.
    /// </summary>
    protected override async void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (e.Source is not Control { DataContext: NodeObserver node } control) return;

        var point = e.GetCurrentPoint(control);

        // ReSharper disable once InvertIf
        if (point.Properties.IsLeftButtonPressed && e.ClickCount == 2)
        {
            e.Handled = true;
            await node.NavigateCommand.ExecuteAsync(null);
        }
    }

    protected override async void OnPointerMoved(PointerEventArgs e)
    {
        if (!IsSelected) return;
        if (e.Source is not Control { DataContext: NodeObserver node } control) return;
        if (!e.GetCurrentPoint(control).Properties.IsLeftButtonPressed) return;

        //Set the data object to this node that is being dragged.
        var data = new DataObject();
        data.Set("Node", node);

        await DragDrop.DoDragDrop(e, data, DragDropEffects.Move);
    }

    /// <summary>
    /// Overriding to prevent the expanding of the node on double click
    /// </summary>
    protected override void OnHeaderDoubleTapped(TappedEventArgs e)
    {
        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Source is not Control { DataContext: NodeObserver node }) return;

        if (e is { Key: Key.E, KeyModifiers: KeyModifiers.Control })
        {
            node.IsEditing = true;
            e.Handled = true;
        }

        if (e.Key is Key.Enter or Key.Escape)
        {
            e.Handled = true;
        }

        base.OnKeyDown(e);
    }

    private async void OnDrop(DragEventArgs e)
    {
        if (e.Source is not Control { DataContext: NodeObserver target }) return;
        if (e.Data.Get("Node") is not NodeObserver dragged) return;
        if (target.Type != NodeType.Container) return;
        if (dragged.Id == target.Id) return;

        UpdateDraggingPseudoClass(false);
        await target.MoveCommand.ExecuteAsync([dragged]);
        e.Handled = true;
    }

    private void OnDragEnter(DragEventArgs e)
    {
        if (e.Source is not Control { DataContext: NodeObserver target }) return;
        if (e.Data.Get("Node") is not NodeObserver dragged) return;
        if (target.Type != NodeType.Container) return;
        if (dragged.Id == target.Id) return;

        UpdateDraggingPseudoClass(true);
        e.Handled = true;
    }

    private void OnDragLeave(DragEventArgs e)
    {
        if (e.Source is not Control { DataContext: NodeObserver target }) return;
        if (e.Data.Get("Node") is not NodeObserver dragged) return;
        if (target.Type != NodeType.Container) return;
        if (dragged.Id == target.Id) return;


        UpdateDraggingPseudoClass(false);
        e.Handled = true;
    }


    private void UpdateDraggingPseudoClass(bool isDragOver)
    {
        if (isDragOver)
        {
            PseudoClasses.Add(PcDragOver);
            return;
        }

        PseudoClasses.Remove(PcDragOver);
    }
}