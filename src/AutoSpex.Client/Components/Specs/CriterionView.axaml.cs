using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoSpex.Client.Shared;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;

// ReSharper disable UnusedParameter.Local

namespace AutoSpex.Client.Components;

public partial class CriterionView : PageView<CriterionViewModel>
{
    public CriterionView()
    {
        InitializeComponent();
        Initialized += OnInitialized;
    }

    private void OnInitialized(object? sender, EventArgs e)
    {
        var propertyInput = this.GetControl<AutoCompleteBox>("PropertyInput");
        propertyInput.AsyncPopulator = PopulateAsync;
        propertyInput.TextSelector = AppendMember;
        
    }

    private Task<IEnumerable<object>> PopulateAsync(string? searchText, CancellationToken cancellationToken)
    {
        //return ViewModel.GetProperties(searchText, cancellationToken);
        throw new NotImplementedException();
    }

    private static string AppendMember(string? text, string? item)
    {
        if (item is null) return string.Empty;

        var parts = text?.Split('.') ?? Array.Empty<string>();

        if (parts.Length == 0) return item;

        parts[^1] = item;
        return string.Join(".", parts);
    }

    private void AutoCompleteGotFocus(object? sender, GotFocusEventArgs e)
    {
        if (e.Source is not TextBox textBox) return;

        Dispatcher.UIThread.Post(() => { textBox.SelectAll(); });
    }

    private void AutoCompleteLostFocus(object? sender, RoutedEventArgs e)
    {
        if (e.Source is not TextBox textBox) return;

        ViewModel.PropertyName = textBox.Text;

        Dispatcher.UIThread.Post(() => { textBox.ClearSelection(); });
    }

    private void AutoCompleteKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Source is not TextBox textBox) return;

        if (e is {Key: Key.Space, KeyModifiers: KeyModifiers.Control})
        {
            var input = this.GetControl<AutoCompleteBox>("PropertyInput");
            input.Text = input.Text?.Trim(' ');
            input.IsDropDownOpen = true;
            return;
        }

        if (e.Key != Key.Escape && e.Key != Key.Enter) return;

        var root = this.GetControl<Border>("Root");
        root.Focus();
    }
    
    private void AutoCompleteKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Source is not TextBox textBox) return;

        if (e is {Key: Key.Space, KeyModifiers: KeyModifiers.Control})
        {
            var input = this.GetControl<AutoCompleteBox>("PropertyInput");
            input.Text = input.Text?.Trim(' ');
            input.IsDropDownOpen = true;
            return;
        }

        if (e.Key != Key.Escape && e.Key != Key.Enter) return;

        var root = this.GetControl<Border>("Root");
        root.Focus();
    }
}