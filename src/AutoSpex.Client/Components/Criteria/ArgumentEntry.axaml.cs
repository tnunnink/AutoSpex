using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Engine;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using DynamicData;

namespace AutoSpex.Client.Components;

public class ArgumentEntry : TemplatedControl
{
    private const string PartFlyoutRoot = "FlyoutRoot";
    private const string PartInputText = "InputText";
    private const string PartSuggestionList = "SuggestionList";

    public static readonly DirectProperty<ArgumentEntry, object?> ValueProperty =
        AvaloniaProperty.RegisterDirect<ArgumentEntry, object?>(
            nameof(Value), o => o.Value, (o, v) => o.Value = v, defaultBindingMode: BindingMode.TwoWay);

    public static readonly DirectProperty<ArgumentEntry, bool> HasValueProperty =
        AvaloniaProperty.RegisterDirect<ArgumentEntry, bool>(
            nameof(HasValue), o => o.HasValue, (o, v) => o.HasValue = v);

    public static readonly DirectProperty<ArgumentEntry, IEnumerable<Argument>> SuggestionSourceProperty =
        AvaloniaProperty.RegisterDirect<ArgumentEntry, IEnumerable<Argument>>(
            nameof(SuggestionSource), o => o.SuggestionSource, (o, v) => o.SuggestionSource = v);

    private object? _value;
    private bool _hasValue;
    private IEnumerable<Argument> _suggestionSource = [];
    private readonly SourceList<Argument> _suggestionsList = new();
    private readonly ReadOnlyObservableCollection<Argument> _suggestions;
    private Border? _flyoutRoot;
    private TextBox? _textBox;
    private ListBox? _listBox;

    public ArgumentEntry()
    {
        _suggestionsList.Connect()
            .Bind(out _suggestions)
            .Subscribe();
    }

    public object? Value
    {
        get => _value;
        set => SetAndRaise(ValueProperty, ref _value, value);
    }

    public bool HasValue
    {
        get => _hasValue;
        set => SetAndRaise(HasValueProperty, ref _hasValue, value);
    }

    public IEnumerable<Argument> SuggestionSource
    {
        get => _suggestionSource;
        set => SetAndRaise(SuggestionSourceProperty, ref _suggestionSource, value);
    }

    public ReadOnlyObservableCollection<Argument> Suggestions => _suggestions;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        RegisterFlyoutRootPart(e);
        RegisterTextBoxPart(e);
        RegisterListBoxPart(e);
        UpdateSuggestions(Value?.ToString() ?? string.Empty);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ValueProperty)
            HandleValueChange(change);
    }

    private void HandleValueChange(AvaloniaPropertyChangedEventArgs change)
    {
        var value = change.NewValue;
        HasValue = value is not null && !string.IsNullOrEmpty(value.ToString());

        if (_textBox is null || value is null) return;
        _textBox.Text = value.ToString();
    }

    private void RegisterFlyoutRootPart(TemplateAppliedEventArgs args)
    {
        _flyoutRoot?.RemoveHandler(KeyDownEvent, HandleKeyDownEvent);
        _flyoutRoot = args.NameScope.Find<Border>(PartFlyoutRoot);
        _flyoutRoot?.AddHandler(KeyDownEvent, HandleKeyDownEvent, RoutingStrategies.Bubble);
    }

    private void RegisterTextBoxPart(TemplateAppliedEventArgs args)
    {
        if (_textBox is not null) _textBox.TextChanged -= OnInputTextChanged;
        _textBox = args.NameScope.Find<TextBox>(PartInputText);
        if (_textBox is null) return;
        _textBox.TextChanged += OnInputTextChanged;
        _textBox.Text = Value?.ToString() ?? string.Empty;
        _textBox.Focus();
        _textBox.SelectAll();
        _textBox.CaretIndex = _textBox.Text?.Length ?? 0;
    }

    private void RegisterListBoxPart(TemplateAppliedEventArgs args)
    {
        _listBox?.RemoveHandler(PointerPressedEvent, HandleListPointerPressed);
        _listBox = args.NameScope.Find<ListBox>(PartSuggestionList);
        _listBox?.AddHandler(PointerPressedEvent, HandleListPointerPressed, RoutingStrategies.Tunnel);
    }

    private void OnInputTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (e.Source is not TextBox textBox) return;
        var text = textBox.Text ?? string.Empty;
        UpdateSuggestions(text);
    }

    private void UpdateSuggestions(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            ApplySuggestions(SuggestionSource);
            return;
        }

        var suggestions = SuggestionSource.Where(a =>
            a.Value.ToString()?.Contains(text, StringComparison.OrdinalIgnoreCase) is true);

        ApplySuggestions(suggestions);
    }

    private void ApplySuggestions(IEnumerable<Argument> suggestions)
    {
        _suggestionsList.Edit(list =>
        {
            list.Clear();
            list.AddRange(suggestions);
        });
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
        if (_listBox is null || _listBox.ItemCount == 0) return;

        if (_listBox.SelectedIndex > 0)
        {
            _listBox.SelectedIndex--;
        }
        else
        {
            _listBox.SelectedIndex = _listBox.ItemCount - 1;
        }
    }

    private void HandleKeyDownNavigation()
    {
        if (_listBox is null || _listBox.ItemCount == 0) return;

        if (_listBox.SelectedIndex < _listBox.ItemCount - 1)
        {
            _listBox.SelectedIndex++;
        }
        else
        {
            _listBox.SelectedIndex = 0;
        }
    }

    private void HandleEscapeNavigation()
    {
        if (_listBox is not null && _listBox.SelectedIndex >= 0)
        {
            _listBox.SelectedIndex = -1;
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
        if (_listBox?.SelectedItem is Argument argument)
        {
            Value = argument.Value;
            return;
        }

        var text = _textBox?.Text;
        if (text is null) return;

        var matching = SuggestionSource.FirstOrDefault(x => x.ToString() == text);
        if (matching is not null)
        {
            Value = matching.Value;
            return;
        }

        Value = text;
    }

    private void CloseFlyout()
    {
        var popup = _flyoutRoot.FindLogicalAncestorOfType<Popup>();
        popup?.Close();
    }
}