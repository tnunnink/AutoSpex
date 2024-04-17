using System;
using AutoSpex.Client.Observers;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AutoSpex.Client.Components;

public class NodeOverview : TemplatedControl
{
    public static readonly DirectProperty<NodeOverview, NodeObserver?> NodeProperty =
        AvaloniaProperty.RegisterDirect<NodeOverview, NodeObserver?>(
            nameof(Node), o => o.Node, (o, v) => o.Node = v);

    private NodeObserver? _node;
    private TextBox? _nameEntry;

    public NodeObserver? Node
    {
        get => _node;
        set => SetAndRaise(NodeProperty, ref _node, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        RegisterNameEntry(e);
    }

    private void RegisterNameEntry(TemplateAppliedEventArgs e)
    {
        _nameEntry?.RemoveHandler(LostFocusEvent, NameEntryLostFocusHandler);
        _nameEntry?.RemoveHandler(KeyDownEvent, NameEntryKeyDownHandler);
        _nameEntry = e.NameScope.Get<TextBox>("NameEntry");
        _nameEntry?.AddHandler(LostFocusEvent, NameEntryLostFocusHandler);
        _nameEntry?.AddHandler(KeyDownEvent, NameEntryKeyDownHandler);
    }

    private async void NameEntryLostFocusHandler(object? sender, RoutedEventArgs e)
    {
        if (e.Source is not TextBox textBox || Node is null) return;
        await Node.RenameCommand.ExecuteAsync(textBox.Text);
        e.Handled = true;
    }

    private async void NameEntryKeyDownHandler(object? sender, KeyEventArgs e)
    {
        if (e.Source is not TextBox textBox || Node is null) return;
        
        if (e.Key == Key.Escape)
        {
            var shell = textBox.FindAncestorOfType<Window>();
            Dispatcher.UIThread.Post(() => { shell?.Focus(); });
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Enter)
        {
            await Node.RenameCommand.ExecuteAsync(textBox.Text);
            e.Handled = true;
        }
    }
}