using System;
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
public partial class RunConfigPage : UserControl
{
    public RunConfigPage()
    {
        InitializeComponent();

        SourceSection.AddHandler(DragDrop.DragEnterEvent, SourceOnDragEnter);
        SourceSection.AddHandler(DragDrop.DragLeaveEvent, SourceOnDragLeave);
        SourceSection.AddHandler(DragDrop.DragOverEvent, SourceOnDragOver);
        SourceSection.AddHandler(DragDrop.DropEvent, NodeOnDrop);
        SpecSection.AddHandler(DragDrop.DragEnterEvent, SpecOnDragEnter);
        SpecSection.AddHandler(DragDrop.DragLeaveEvent, SpecOnDragLeave);
        SpecSection.AddHandler(DragDrop.DragOverEvent, SpecOnDragOver);
        SpecSection.AddHandler(DragDrop.DropEvent, NodeOnDrop);
    }

    private void SourceOnDragEnter(object? sender, DragEventArgs e)
    {
        if (!IsValidSourceNode(sender, e))
        {
            e.DragEffects = DragDropEffects.None;
            return;
        }

        SourceSection.BorderBrush = GetBrush(ThemeResourceKind.ControlBackgroundBrushEmphasizedAccent.ToResourceKey());
        e.Handled = true;
    }

    private void SourceOnDragLeave(object? sender, DragEventArgs e)
    {
        if (!IsValidSourceNode(sender, e))
        {
            e.DragEffects = DragDropEffects.None;
            return;
        }

        SourceSection.BorderBrush = GetBrush(ThemeResourceKind.Container1BorderBrush.ToResourceKey());
        e.Handled = true;
    }

    private static void SourceOnDragOver(object? sender, DragEventArgs e)
    {
        if (!IsValidSourceNode(sender, e))
        {
            e.DragEffects = DragDropEffects.None;
            return;
        }

        e.Handled = true;
    }

    private void SpecOnDragEnter(object? sender, DragEventArgs e)
    {
        if (!IsValidSpecNode(sender, e))
        {
            e.DragEffects = DragDropEffects.None;
            return;
        }

        SpecSection.BorderBrush = GetBrush(ThemeResourceKind.ControlBackgroundBrushEmphasizedAccent.ToResourceKey());
        e.Handled = true;
    }

    private void SpecOnDragLeave(object? sender, DragEventArgs e)
    {
        if (!IsValidSpecNode(sender, e))
        {
            e.DragEffects = DragDropEffects.None;
            return;
        }

        SpecSection.BorderBrush = GetBrush(ThemeResourceKind.Container1BorderBrush.ToResourceKey());
        e.Handled = true;
    }

    private static void SpecOnDragOver(object? sender, DragEventArgs e)
    {
        if (!IsValidSpecNode(sender, e))
        {
            e.DragEffects = DragDropEffects.None;
            return;
        }

        e.Handled = true;
    }

    private void NodeOnDrop(object? sender, DragEventArgs e)
    {
        if (e.DragEffects != DragDropEffects.Move) return;
        if (sender is not Control { DataContext: RunConfigPageModel page }) return;
        if (e.Data.Get("Node") is not NodeObserver node) return;

        page.AddNode(node);

        SourceSection.BorderBrush = GetBrush(ThemeResourceKind.Container1BorderBrush.ToResourceKey());
        SpecSection.BorderBrush = GetBrush(ThemeResourceKind.Container1BorderBrush.ToResourceKey());
        e.Handled = true;
    }

    private static bool IsValidSpecNode(object? sender, DragEventArgs e)
    {
        if (sender is not Control { DataContext: RunConfigPageModel }) return false;
        return e.Data.Get("Node") is NodeObserver node && node.Feature == NodeType.Spec;
    }

    private static bool IsValidSourceNode(object? sender, DragEventArgs e)
    {
        if (sender is not Control { DataContext: RunConfigPageModel }) return false;
        return e.Data.Get("Node") is NodeObserver node && node.Feature == NodeType.Source;
    }

    private static IBrush GetBrush(string key)
    {
        object? resource = default;
        var found = Application.Current?.TryGetResource(key, Application.Current.ActualThemeVariant, out resource);
        return found is true && resource is IBrush brush ? brush : new SolidColorBrush(Colors.Transparent);
    }
}