using System;
using System.Collections.ObjectModel;
using AutoSpex.Client.Observers;
using AutoSpex.Engine;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;

namespace AutoSpex.Client.Components;

public class ArgumentEntry : TemplatedControl
{
    private const string PartFlyoutRoot = "FlyoutRoot";
    private const string PartInputText = "InputText";
    private const string PartSuggestionList = "SuggestionList";

    public static readonly DirectProperty<ArgumentEntry, ArgumentObserver?> ArgumentProperty =
        AvaloniaProperty.RegisterDirect<ArgumentEntry, ArgumentObserver?>(
            nameof(Argument), o => o.Argument, (o, v) => o.Argument = v);

    private ArgumentObserver? _argument;
    private Border? _flyoutRoot;
    private TextBox? _inputText;
    private ListBox? _suggestionList;

    public ArgumentObserver? Argument
    {
        get => _argument;
        set => SetAndRaise(ArgumentProperty, ref _argument, value);
    }

    public ObservableCollection<Argument> Suggestions { get; } = [];

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        RegisterFlyoutRootPart(e);
        RegisterTextBoxPart(e);
        RegisterListBoxPart(e);
    }

    private void RegisterFlyoutRootPart(TemplateAppliedEventArgs args)
    {
        _flyoutRoot?.RemoveHandler(KeyDownEvent, HandleKeyDownEvent);

        _flyoutRoot = args.NameScope.Find<Border>(PartFlyoutRoot);
        if (_flyoutRoot is null) return;

        _flyoutRoot.AddHandler(KeyDownEvent, HandleKeyDownEvent, RoutingStrategies.Bubble);
        _flyoutRoot.AttachedToVisualTree += HandleFlyoutAttached;
    }

    private async void HandleFlyoutAttached(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (_inputText is null) return;

        _inputText.Text = Argument?.Formatted ?? string.Empty;
        _inputText.Focus();
        _inputText.SelectAll();
        _inputText.CaretIndex = _inputText.Text?.Length ?? 0;

        await UpdateSuggestions(_inputText.Text);
    }

    private void RegisterTextBoxPart(TemplateAppliedEventArgs args)
    {
        if (_inputText is not null) _inputText.TextChanged -= OnInputTextChanged;
        _inputText = args.NameScope.Find<TextBox>(PartInputText);
        if (_inputText is null) return;
        _inputText.TextChanged += OnInputTextChanged;
    }

    private void RegisterListBoxPart(TemplateAppliedEventArgs args)
    {
        _suggestionList?.RemoveHandler(PointerPressedEvent, HandleListPointerPressed);
        _suggestionList = args.NameScope.Find<ListBox>(PartSuggestionList);
        _suggestionList?.AddHandler(PointerPressedEvent, HandleListPointerPressed, RoutingStrategies.Tunnel);
    }

    private async void OnInputTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (e.Source is not TextBox textBox) return;
        var text = textBox.Text ?? string.Empty;
        await UpdateSuggestions(text);
    }

    private async Task UpdateSuggestions(string? text)
    {
        Suggestions.Clear();

        if (Argument is null) return;

        try
        {
            var suggestions = await Argument.GetSuggestions(text);

            foreach (var suggestion in suggestions)
                Suggestions.Add(suggestion);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private void HandleKeyDownEvent(object? sender, KeyEventArgs args)
    {
        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (args.Key)
        {
            case Key.Down:
                HandleKeyDownNavigation();
                args.Handled = true;
                break;
            case Key.Up:
                HandleKeyUpNavigation();
                args.Handled = true;
                break;
            case Key.Escape:
                HandleEscapeNavigation();
                args.Handled = true;
                break;
            case Key.Tab:
                UpdateValue();
                args.Handled = true;
                break;
            case Key.Enter:
                UpdateAndClose();
                args.Handled = true;
                break;
        }
    }

    private void HandleKeyUpNavigation()
    {
        if (_suggestionList is null || _suggestionList.ItemCount == 0) return;

        if (_suggestionList.SelectedIndex > 0)
        {
            _suggestionList.SelectedIndex--;
        }
        else
        {
            _suggestionList.SelectedIndex = _suggestionList.ItemCount - 1;
        }
    }

    private void HandleKeyDownNavigation()
    {
        if (_suggestionList is null || _suggestionList.ItemCount == 0) return;

        if (_suggestionList.SelectedIndex < _suggestionList.ItemCount - 1)
        {
            _suggestionList.SelectedIndex++;
        }
        else
        {
            _suggestionList.SelectedIndex = 0;
        }
    }

    private void HandleEscapeNavigation()
    {
        if (_suggestionList is not null && _suggestionList.SelectedIndex >= 0)
        {
            _suggestionList.SelectedIndex = -1;
            return;
        }

        CloseFlyout();
    }

    private void HandleListPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e is not {ClickCount: 2}) return;
        e.Handled = true;
        UpdateAndClose();
    }

    private void UpdateAndClose()
    {
        UpdateValue();
        CloseFlyout();
    }

    private void UpdateValue()
    {
        if (Argument is null) return;

        if (_suggestionList?.SelectedItem is Argument argument)
        {
            Argument.Value = argument.Value;
            return;
        }

        Argument.ResolveInput(_inputText?.Text);
    }

    private void CloseFlyout()
    {
        var popup = _flyoutRoot.FindLogicalAncestorOfType<Popup>();
        popup?.Close();
    }
}