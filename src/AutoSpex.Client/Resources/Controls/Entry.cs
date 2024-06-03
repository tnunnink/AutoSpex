using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace AutoSpex.Client.Resources.Controls;

[PseudoClasses(ClassEmpty)]
public class Entry : TemplatedControl
{
    #region Properties

    public static readonly StyledProperty<object?> ValueProperty =
        AvaloniaProperty.Register<Entry, object?>(
            nameof(Value), defaultBindingMode: BindingMode.TwoWay, enableDataValidation: true);

    public static readonly StyledProperty<IDataTemplate?> ValueTemplateProperty =
        AvaloniaProperty.Register<Entry, IDataTemplate?>(
            nameof(ValueTemplate));

    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<Entry, string?>(
            nameof(Text));

    public static readonly StyledProperty<string?> WatermarkProperty =
        AvaloniaProperty.Register<Entry, string?>(
            nameof(Watermark));

    public static readonly StyledProperty<bool> IsEmptyProperty =
        AvaloniaProperty.Register<Entry, bool>(
            nameof(IsEmpty));

    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        AvaloniaProperty.Register<Entry, IDataTemplate?>(
            nameof(ItemTemplate));

    public static readonly StyledProperty<bool> IsDropDownOpenProperty =
        AvaloniaProperty.Register<Entry, bool>(
            nameof(IsDropDownOpen));

    public static readonly StyledProperty<int> MaxDropDownHeightProperty =
        AvaloniaProperty.Register<Entry, int>(
            nameof(MaxDropDownHeight));

    public static readonly StyledProperty<int> MinDropDownWidthProperty =
        AvaloniaProperty.Register<Entry, int>(
            nameof(MinDropDownWidth));

    public static readonly StyledProperty<int> MaxDropDownWidthProperty =
        AvaloniaProperty.Register<Entry, int>(
            nameof(MaxDropDownWidth));

    public static readonly StyledProperty<Func<string?, CancellationToken, Task<IEnumerable<object>>>?>
        PopulateProperty =
            AvaloniaProperty.Register<Entry, Func<string?, CancellationToken, Task<IEnumerable<object>>>?>(
                nameof(Populate));

    public static readonly StyledProperty<Func<object?, string>?> SelectorProperty =
        AvaloniaProperty.Register<Entry, Func<object?, string>?>(
            nameof(Selector));

    public static readonly StyledProperty<Action<object?>?> CommitProperty =
        AvaloniaProperty.Register<Entry, Action<object?>?>(
            nameof(Commit));

    #endregion

    private const string ClassEmpty = ":empty";
    private const string PartTextBox = "PART_TextBox";
    private const string PartListBox = "PART_ListBox";
    private TextBox? _textBox;
    private ListBox? _listBox;

    static Entry()
    {
        PointerReleasedEvent.AddClassHandler<Entry>((e, a) => e.HandlePointerReleased(a), RoutingStrategies.Tunnel);
    }

    public object? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public IDataTemplate? ValueTemplate
    {
        get => GetValue(ValueTemplateProperty);
        set => SetValue(ValueTemplateProperty, value);
    }

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string? Watermark
    {
        get => GetValue(WatermarkProperty);
        set => SetValue(WatermarkProperty, value);
    }

    public bool IsEmpty
    {
        get => GetValue(IsEmptyProperty);
        set => SetValue(IsEmptyProperty, value);
    }

    public IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public bool IsDropDownOpen
    {
        get => GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }

    public int MaxDropDownHeight
    {
        get => GetValue(MaxDropDownHeightProperty);
        set => SetValue(MaxDropDownHeightProperty, value);
    }

    public int MaxDropDownWidth
    {
        get => GetValue(MaxDropDownWidthProperty);
        set => SetValue(MaxDropDownWidthProperty, value);
    }

    public int MinDropDownWidth
    {
        get => GetValue(MinDropDownWidthProperty);
        set => SetValue(MinDropDownWidthProperty, value);
    }

    public Func<string?, CancellationToken, Task<IEnumerable<object>>>? Populate
    {
        get => GetValue(PopulateProperty);
        set => SetValue(PopulateProperty, value);
    }

    public Func<object?, string>? Selector
    {
        get => GetValue(SelectorProperty);
        set => SetValue(SelectorProperty, value);
    }

    public Action<object?>? Commit
    {
        get => GetValue(CommitProperty);
        set => SetValue(CommitProperty, value);
    }

    public ObservableCollection<object> Suggestions { get; } = [];

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ValueProperty && IsDropDownOpen)
            SelectText(change.GetNewValue<object?>());

        if (change.Property == TextProperty)
            UpdateSuggestions(change.GetNewValue<string?>());

        if (change.Property == IsDropDownOpenProperty)
            HandleDropDownOpenChange(change.GetNewValue<bool>());

        if (change.Property == IsEmptyProperty)
            UpdateVisualStateForIsEmpty(change.GetNewValue<bool>());
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        RegisterTextBox(e);
        RegisterListBox(e);
    }

    /// <inheritdoc />
    protected override void UpdateDataValidation(AvaloniaProperty property, BindingValueType state, Exception? error)
    {
        base.UpdateDataValidation(property, state, error);

        if (property != ValueProperty) return;
        DataValidationErrors.SetError(this, error);
    }

    /// <summary>
    /// Registers the entry text box and attaches the key down event handler to handler navigation of the suggestion list.
    /// </summary>
    private void RegisterTextBox(TemplateAppliedEventArgs args)
    {
        _textBox?.RemoveHandler(KeyDownEvent, OnTextBoxKeyDown);
        _textBox = args.NameScope.Get<TextBox>(PartTextBox);
        _textBox.AddHandler(KeyDownEvent, OnTextBoxKeyDown, RoutingStrategies.Tunnel);
    }

    /// <summary>
    /// Registers the suggestion list box and attached the pointer released event handler to trigger the commit when selected.
    /// </summary>
    private void RegisterListBox(TemplateAppliedEventArgs args)
    {
        _listBox?.RemoveHandler(PointerPressedEvent, OnListBoxPointerReleased);
        _listBox = args.NameScope.Get<ListBox>(PartListBox);
        _listBox.AddHandler(PointerReleasedEvent, OnListBoxPointerReleased, RoutingStrategies.Tunnel);
    }

    private void OnTextBoxKeyDown(object? sender, KeyEventArgs e)
    {
        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (e.Key)
        {
            case Key.Down:
                HandleDownNavigation();
                e.Handled = true;
                break;
            case Key.Up:
                HandleUpNavigation();
                e.Handled = true;
                break;
            case Key.Escape:
                HandleEscapeNavigation();
                e.Handled = true;
                break;
            case Key.Tab:
                HandleTabNavigation();
                e.Handled = true;
                break;
            case Key.Enter:
                CommitValue();
                e.Handled = true;
                break;
        }
    }

    /// <summary>
    /// Handles navigation of the suggested items list. When the user presses the up key, cycle up the list.
    /// </summary>
    private void HandleUpNavigation()
    {
        if (_listBox is null) return;

        if (_listBox.SelectedIndex > 0)
        {
            _listBox.SelectedIndex--;
        }
        else
        {
            _listBox.SelectedIndex = _listBox.ItemCount - 1;
        }
    }

    /// <summary>
    /// Handles navigation of the suggested items list. When the user presses the down key, cycle down the list.
    /// </summary>
    private void HandleDownNavigation()
    {
        if (_listBox is null) return;

        if (_listBox.SelectedIndex < _listBox.ItemCount - 1)
        {
            _listBox.SelectedIndex++;
        }
        else
        {
            _listBox.SelectedIndex = 0;
        }
    }

    /// <summary>
    /// Handles navigation of the suggested items list. When the user presses the escape key, we exit without commiting a value.
    /// </summary>
    private void HandleEscapeNavigation()
    {
        if (_listBox is not null && _listBox.SelectedIndex >= 0)
        {
            _listBox.SelectedIndex = -1;
        }

        ClosePopup();
    }

    /// <summary>
    /// Handles navigation of the suggested items list. When the user presses the tab key, we will commit the selected
    /// value but not close the flyout. This allows effectively updates the text entry with the selected value and allows
    /// continuation of field entry, which we need to dot down path entry.
    /// </summary>
    private void HandleTabNavigation()
    {
        if (_listBox is null) return;

        //First ensure a selection by advancing the selected index of the list box if no selection is made.
        if (_listBox.SelectedIndex < 0 && _listBox.ItemCount > 0)
        {
            _listBox.SelectedIndex++;
        }

        //Get the selected item which we should now have.
        var selected = _listBox.SelectedItem;

        //Use that to update the text box and advance the caret to the end of the string.
        //Updating the text will in turn populate the suggestions again with the new text.
        SelectText(selected);
        AdvanceCaret();
    }

    /// <summary>
    /// If the control loses focus we want to validate the current content value in order to trigger UI update in the
    /// case that the value has errors.
    /// </summary>
    private void HandleDropDownOpenChange(bool open)
    {
        if (!open) return;
        SelectText(Value);
        UpdateSuggestions(Text);
    }

    /// <summary>
    /// Handles the list box pointer released event by commiting the pressed item and closing the flyout
    /// </summary>
    private void OnListBoxPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        e.Handled = true;
        CommitValue();
    }

    /// <summary>
    /// Sets the caret of the entry text box to the end of the string.
    /// </summary>
    private void AdvanceCaret()
    {
        if (_textBox is null) return;
        _textBox.CaretIndex = _textBox.Text?.Length ?? 0;
    }

    /// <summary>
    /// Converts the provided value to a text representation that can update the local text box control.
    /// </summary>
    /// <param name="value">The value to update.</param>
    private void SelectText(object? value)
    {
        if (Selector is null)
        {
            Text = value?.ToString() ?? string.Empty;
            return;
        }

        Text = Selector.Invoke(value);
    }

    /// <summary>
    /// Executes the commit action bound to this control. This allows the user to update the underlying value of the
    /// object this entry represents, which is expected to be the content of the control.
    /// </summary>
    private void CommitValue()
    {
        var value = _listBox?.SelectedItem ?? Text;

        if (Commit is not null)
        {
            Commit?.Invoke(value);
        }
        else
        {
            Value = value;
        }

        ClosePopup();
    }

    /// <summary>
    /// Executes the attached populate funtion and updates the local <see cref="Suggestions"/> collection with the results.
    /// </summary>
    /// <param name="text">The current text to filter the returned suggestions.</param>
    private async void UpdateSuggestions(string? text)
    {
        Suggestions.Clear();

        if (Populate is null) return;
        var results = await Populate.Invoke(text, CancellationToken.None);

        foreach (var result in results)
        {
            Suggestions.Add(result);
        }
    }

    /// <summary>
    /// Class event handler for the pointer released which triggers the popup to open.
    /// </summary>
    private void HandlePointerReleased(RoutedEventArgs e)
    {
        if (e.Source is not Control control || control.FindAncestorOfType<Button>() is null) return;
        IsDropDownOpen = true;
        e.Handled = true;
    }

    /// <summary>
    /// Update the pseudo classes based on the provided empty state flag. This class controls the visibility
    /// of our watermark text in the control theme. There were issues IsVisible of ContentPresenter directly to
    /// the IsEmpty property, but I was able to get this working with a pseudo class.
    /// </summary>
    private void UpdateVisualStateForIsEmpty(bool empty)
    {
        if (empty)
        {
            PseudoClasses.Add(ClassEmpty);
            return;
        }

        PseudoClasses.Remove(ClassEmpty);
    }

    /// <summary>
    /// Closes the popup control.
    /// </summary>
    private void ClosePopup()
    {
        IsDropDownOpen = false;
    }
}