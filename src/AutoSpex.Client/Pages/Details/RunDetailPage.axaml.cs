using AutoSpex.Client.Observers;
using Avalonia.Controls;
using Avalonia.Input;

namespace AutoSpex.Client.Pages;

public partial class RunDetailPage : UserControl
{
    private const string ClassDragOver = "dragover";

    public RunDetailPage()
    {
        InitializeComponent();

        DropArea.AddHandler(DragDrop.DragEnterEvent, DragEnterHandler);
        DropArea.AddHandler(DragDrop.DragLeaveEvent, DragLeaveHandler);
        DropArea.AddHandler(DragDrop.DragOverEvent, DragOverHandler);
        DropArea.AddHandler(DragDrop.DropEvent, DropHandler);
    }

    /// <summary>
    /// Handle the node drag enter event for the control.
    /// </summary>
    private void DragEnterHandler(object? sender, DragEventArgs e)
    {
        if (!TryGetNode(e, out _)) return;
        UpdateVisualStateDragOver(true);
        e.Handled = true;
    }

    /// <summary>
    /// Handle the node drag leave event for the control.
    /// </summary>
    private void DragLeaveHandler(object? sender, DragEventArgs e)
    {
        if (!TryGetNode(e, out _)) return;
        UpdateVisualStateDragOver(false);
        e.Handled = true;
    }

    /// <summary>
    /// Handle the node drag over event for the control.
    /// </summary>
    private static void DragOverHandler(object? sender, DragEventArgs e)
    {
        if (!TryGetNode(e, out _)) return;
        e.Handled = true;
    }

    /// <summary>
    /// When the user drops a valid node object on this control, we want to either create a new run and seed it with
    /// the dropped node, or simply add the node to the current run instance.
    /// </summary>
    private void DropHandler(object? sender, DragEventArgs e)
    {
        if (!TryGetNode(e, out var node)) return;
        if (node is null) return;
        if (e.Source is not Control { DataContext: RunDetailPageModel page }) return;

        //Adds all nodes to the run.
        page.Run.AddNodeCommand.Execute(node);

        UpdateVisualStateDragOver(false);
        e.Handled = true;
    }

    /// <summary>
    /// Determine if the current drag over event contains a valid node which can be dropped on this control.
    /// If valid, return the node instance along with the flag.
    /// </summary>
    private static bool TryGetNode(DragEventArgs e, out NodeObserver? node)
    {
        if (e.Data.Get("Node") is NodeObserver observer)
        {
            node = observer;
            return true;
        }

        node = null;
        return false;
    }

    /// <summary>
    /// Update the visual state to indicate that the control is a valid drop location for the dragged node.
    /// </summary>
    private void UpdateVisualStateDragOver(bool dragover)
    {
        if (dragover)
        {
            DropArea.Classes.Add(ClassDragOver);
            return;
        }

        DropArea.Classes.Remove(ClassDragOver);
    }
}