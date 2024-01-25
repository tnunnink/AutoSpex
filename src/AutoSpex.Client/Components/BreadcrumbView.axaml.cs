using AutoSpex.Client.Observers;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;

// ReSharper disable UnusedParameter.Local
// ReSharper disable SuggestBaseTypeForParameter

namespace AutoSpex.Client.Components;

public partial class BreadcrumbView : UserControl
{
    public BreadcrumbView()
    {
        InitializeComponent();
    }

    private void CrumbEntryGotFocus(object? sender, GotFocusEventArgs e)
    {
        if (e.Source is not TextBox textBox) return;
        
        Dispatcher.UIThread.Post(() =>
        {
            textBox.SelectAll();
        });
    }

    private void CrumbEntryLostFocus(object? sender, RoutedEventArgs e)
    {
        if (e.Source is not TextBox {DataContext: Breadcrumb breadcrumb} textBox) return;
        
        breadcrumb.ResetName();
        
        Dispatcher.UIThread.Post(() =>
        {
            textBox.ClearSelection();
        });
        
        breadcrumb.InFocus = false;
    }

    private void CrumbEntryKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Source is not TextBox {DataContext: Breadcrumb breadcrumb}) return;

        if (e.Key != Key.Escape && e.Key != Key.Enter) return;
        
        var itemsControl = this.GetControl<ItemsControl>("ItemsControl");
        
        if (e.Key == Key.Escape)
        {
            breadcrumb.ResetName();
            itemsControl.Focus();
            return;
        }
        
        breadcrumb.RenameCommand.Execute(null);
        itemsControl.Focus();
    }
}