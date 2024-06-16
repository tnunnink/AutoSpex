using ActiproSoftware.UI.Avalonia.Themes;
using AutoSpex.Client.Observers;
using AutoSpex.Engine;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class RunResultsPage : UserControl
{
    public RunResultsPage()
    {
        InitializeComponent();

        DropSection.AddHandler(DragDrop.DragEnterEvent, NodeOnDragEnter);
        DropSection.AddHandler(DragDrop.DragLeaveEvent, NodeOnDragLeave);
        DropSection.AddHandler(DragDrop.DragOverEvent, NodeOnDragOver);
        DropSection.AddHandler(DragDrop.DropEvent, NodeOnDrop);
    }

    private void NodeOnDragEnter(object? sender, DragEventArgs e)
    {
        if (!IsValidNode(sender, e))
        {
            e.DragEffects = DragDropEffects.None;
            return;
        }

        DropSection.BorderBrush = GetBrush(ThemeResourceKind.ControlBackgroundBrushEmphasizedAccent.ToResourceKey());
        e.Handled = true;
    }

    private void NodeOnDragLeave(object? sender, DragEventArgs e)
    {
        if (!IsValidNode(sender, e))
        {
            e.DragEffects = DragDropEffects.None;
            return;
        }

        DropSection.BorderBrush = GetBrush(ThemeResourceKind.Container1BorderBrush.ToResourceKey());
        e.Handled = true;
    }

    private static void NodeOnDragOver(object? sender, DragEventArgs e)
    {
        if (!IsValidNode(sender, e))
        {
            e.DragEffects = DragDropEffects.None;
            return;
        }

        e.Handled = true;
    }

    private void NodeOnDrop(object? sender, DragEventArgs e)
    {
        if (e.DragEffects != DragDropEffects.Move) return;
        if (sender is not Control { DataContext: RunResultsPageModel page }) return;
        if (e.Data.Get("Node") is not NodeObserver node) return;

        page.AddNode(node);

        DropSection.BorderBrush = GetBrush(ThemeResourceKind.Container1BorderBrush.ToResourceKey());
        e.Handled = true;
    }

    private static bool IsValidNode(object? sender, DragEventArgs e)
    {
        if (sender is not Control { DataContext: RunResultsPageModel }) return false;
        return e.Data.Get("Node") is NodeObserver node &&
               (node.Feature == NodeType.Source || node.Feature == NodeType.Spec);
    }

    private static IBrush GetBrush(string key)
    {
        object? resource = default;
        var found = Application.Current?.TryGetResource(key, Application.Current.ActualThemeVariant, out resource);
        return found is true && resource is IBrush brush ? brush : new SolidColorBrush(Colors.Transparent);
    }
}