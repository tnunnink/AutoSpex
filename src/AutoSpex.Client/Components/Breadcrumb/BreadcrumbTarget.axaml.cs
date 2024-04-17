using AutoSpex.Client.Observers;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AutoSpex.Client.Components;

public class BreadcrumbTarget : TemplatedControl
{
    private const string PartEntry = "NameEntry";
    private TextBox? _entry;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        RegisterEntryEvents(e);
    }

    private void RegisterEntryEvents(TemplateAppliedEventArgs args)
    {
        _entry?.RemoveHandler(GotFocusEvent, EntryGotFocus);
        _entry?.RemoveHandler(LostFocusEvent, EntryLostFocus);
        _entry?.RemoveHandler(KeyUpEvent, EntryKeyUp);

        _entry = args.NameScope.Find<TextBox>(PartEntry);
        
        _entry?.AddHandler(GotFocusEvent, EntryGotFocus);
        _entry?.AddHandler(LostFocusEvent, EntryLostFocus);
        _entry?.AddHandler(KeyUpEvent, EntryKeyUp);
    }

    private static void EntryGotFocus(object? sender, GotFocusEventArgs e)
    {
        if (e.Source is not TextBox textBox) return;
        Dispatcher.UIThread.Post(() => { textBox.SelectAll(); });
    }

    private static void EntryLostFocus(object? sender, RoutedEventArgs e)
    {
        if (e.Source is not TextBox {DataContext: Breadcrumb breadcrumb} textBox) return;

        breadcrumb.Name = breadcrumb.Model.Name;
        breadcrumb.InFocus = false;
        textBox.ClearSelection();
    }

    private static async void EntryKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Source is not TextBox {DataContext: Breadcrumb breadcrumb} textBox) return;
        if (e.Key != Key.Escape && e.Key != Key.Enter) return;
        
        if (e.Key == Key.Enter)
        {
            await breadcrumb.RenameCommand.ExecuteAsync(textBox.Text);
            breadcrumb.InFocus = false;
        }
        
        var shell = textBox.FindAncestorOfType<Window>();
        Dispatcher.UIThread.Post(() => { shell?.Focus(); });
    }
}