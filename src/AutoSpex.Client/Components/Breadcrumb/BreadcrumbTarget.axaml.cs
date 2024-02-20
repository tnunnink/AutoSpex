using AutoSpex.Client.Observers;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AutoSpex.Client.Components;

[TemplatePart(PartEntry, typeof(TextBox))]
public class BreadcrumbTarget : TemplatedControl
{
    private const string PartEntry = "PART_TextEntry";
    private TextBox? _entry;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        RegisterEntryEvents(e);
    }

    private void RegisterEntryEvents(TemplateAppliedEventArgs args)
    {
        if (_entry is not null)
        {
            _entry.GotFocus -= EntryGotFocus;
            _entry.LostFocus -= EntryLostFocus;
            _entry.KeyUp -= EntryKeyUp;
        }

        _entry = args.NameScope.Find(PartEntry) as TextBox;

        if (_entry is null) return;
        _entry.GotFocus += EntryGotFocus;
        _entry.LostFocus += EntryLostFocus;
        _entry.KeyUp += EntryKeyUp;
    }

    private static void EntryGotFocus(object? sender, GotFocusEventArgs e)
    {
        if (e.Source is not TextBox textBox) return;

        Dispatcher.UIThread.Post(() => { textBox.SelectAll(); });
    }

    private static void EntryLostFocus(object? sender, RoutedEventArgs e)
    {
        if (e.Source is not TextBox {DataContext: Breadcrumb breadcrumb} textBox) return;

        breadcrumb.Model.Refresh();
        
        Dispatcher.UIThread.Post(() => { textBox.ClearSelection(); });
        
        breadcrumb.InFocus = false;
    }

    private static void EntryKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Source is not TextBox {DataContext: Breadcrumb breadcrumb} textBox) return;
        if (e.Key != Key.Escape && e.Key != Key.Enter) return;
        
        if (e.Key == Key.Enter)
        {
            breadcrumb.RenameCommand.Execute(null);
            breadcrumb.InFocus = false;
        }
        
        var shell = textBox.FindAncestorOfType<Window>();
        Dispatcher.UIThread.Post(() => { shell?.Focus(); });
    }
}