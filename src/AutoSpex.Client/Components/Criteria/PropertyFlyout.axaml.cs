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

public class PropertyFlyout : TemplatedControl
{
    #region Properties
    
    public static readonly DirectProperty<PropertyFlyout, Type?> OriginTypeProperty =
        AvaloniaProperty.RegisterDirect<PropertyFlyout, Type?>(
            nameof(OriginType), o => o.OriginType, (o, v) => o.OriginType = v);

    public static readonly DirectProperty<PropertyFlyout, string?> PropertyNameProperty =
        AvaloniaProperty.RegisterDirect<PropertyFlyout, string?>(
            nameof(PropertyName), o => o.PropertyName, (o, v) => o.PropertyName = v,
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly DirectProperty<PropertyFlyout, bool> IsResolvableProperty =
        AvaloniaProperty.RegisterDirect<PropertyFlyout, bool>(
            nameof(IsResolvable), o => o.IsResolvable, (o, v) => o.IsResolvable = v);

    #endregion
    
    private const string PartEntry = "PART_PropertyEntry";
    private const string PartList = "PART_PropertyList";
    private Type? _originType;
    private string? _propertyName;
    private bool _isResolvable;
    private readonly SourceList<Property> _source = new();
    private readonly ReadOnlyObservableCollection<Property> _properties;
    private TextBox? _textBox;
    private ListBox? _listBox;

    public PropertyFlyout()
    {
        _source.Connect()
            .Sort(LevenshteinComparer<Property>.For(p => p.Name, PropertyName))
            .Bind(out _properties)
            .Subscribe();
    }

    static PropertyFlyout()
    {
        KeyDownEvent.AddClassHandler<PropertyFlyout>(
            (c, a) => c.HandleKeyDownEvent(a),
            RoutingStrategies.Tunnel);
    }

    public ReadOnlyObservableCollection<Property> Properties => _properties;

    public Type? OriginType
    {
        get => _originType;
        set => SetAndRaise(OriginTypeProperty, ref _originType, value);
    }

    public string? PropertyName
    {
        get => _propertyName;
        set => SetAndRaise(PropertyNameProperty, ref _propertyName, value);
    }

    public bool IsResolvable
    {
        get => _isResolvable;
        set => SetAndRaise(IsResolvableProperty, ref _isResolvable, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        RegisterTextBox(e);
        RegisterListBox(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == OriginTypeProperty)
            UpdatePropertySource(PropertyName);
    }

    private void RegisterTextBox(TemplateAppliedEventArgs args)
    {
        if (_textBox is not null)
        {
            _textBox.TextChanged -= PropertyEntryTextChanged;
        }

        _textBox = args.NameScope.Find<TextBox>(PartEntry);

        if (_textBox is null) return;
        _textBox.TextChanged += PropertyEntryTextChanged;
        _textBox.Text = PropertyName;
        _textBox.Focus();
        _textBox.SelectAll();
        _textBox.CaretIndex = _textBox.Text?.Length ?? 0;
    }

    private void RegisterListBox(TemplateAppliedEventArgs args)
    {
        _listBox?.RemoveHandler(PointerPressedEvent, HandleListPointerPressed);

        _listBox = args.NameScope.Find<ListBox>(PartList);

        if (_listBox is null) return;
        _listBox.SelectedIndex = 0;
        _listBox.AddHandler(PointerPressedEvent, HandleListPointerPressed, RoutingStrategies.Tunnel);
    }

    private void PropertyEntryTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (e.Source is not TextBox textBox) return;
        UpdatePropertySource(textBox.Text);
    }

    private void HandleKeyDownEvent(KeyEventArgs args)
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
                AppendSelected();
                args.Handled = true;
                break;
            case Key.Enter:
                AppendAndClose();
                args.Handled = true;
                break;
        }
    }

    private void HandleKeyUpNavigation()
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

    private void HandleKeyDownNavigation()
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

    private void HandleEscapeNavigation()
    {
        if (_listBox is not null && _listBox.SelectedIndex >= 0)
        {
            _listBox.SelectedIndex = -1;
            return;
        }

        CloseContainingPopup();
    }

    private void HandleListPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e is not {ClickCount: 2}) return;
        e.Handled = true;
        AppendAndClose();
    }

    private void UpdatePropertySource(string? propertyName)
    {
        _source.Edit(l =>
        {
            l.Clear();
            l.AddRange(GetProperties(propertyName));
        });

        if (_listBox is null) return;
        _listBox.SelectedIndex = 0;

        ResolveProperty();
    }

    private IEnumerable<Property> GetProperties(string? propertyName)
    {
        if (OriginType is null)
            return Enumerable.Empty<Property>();

        if (string.IsNullOrEmpty(propertyName))
            return OriginType.Properties();

        var index = propertyName.LastIndexOf('.');
        var path = index > -1 ? propertyName[..index] : string.Empty;
        var member = index > -1 ? propertyName[(index + 1)..] : propertyName;

        var property = OriginType.Property(path);
        var properties = property?.Properties ?? OriginType.Properties();

        return properties
            .Where(p => p.Name.Contains(member, StringComparison.OrdinalIgnoreCase))
            .OrderBy(p => p.Name);
    }

    private void AppendAndClose()
    {
        AppendSelected();
        PropertyName = _textBox?.Text;
        CloseContainingPopup();
    }

    private void AppendSelected()
    {
        if (_textBox is null) return;
        if (_listBox?.SelectedItem is not Property property) return;

        var text = _textBox.Text;
        var selectedName = property.Name;

        var parts = text?.Split('.') ?? Array.Empty<string>();

        if (parts.Length == 0)
        {
            _textBox.Text = selectedName;
            return;
        }

        parts[^1] = selectedName;
        _textBox.Text = string.Join(".", parts);
        _textBox.CaretIndex = _textBox.Text?.Length ?? 0;
    }

    private void ResolveProperty()
    {
        if (string.IsNullOrEmpty(PropertyName)) return;
        var property = OriginType?.Property(PropertyName);
        IsResolvable = property is not null;
    }

    private void CloseContainingPopup()
    {
        var popup = this.FindLogicalAncestorOfType<Popup>();
        popup?.Close();
    }
}

public class LevenshteinComparer<T> : IComparer<T>
{
    private readonly Func<T, string?> _selector;
    private readonly string? _input;

    private LevenshteinComparer(Func<T, string?> selector, string? input)
    {
        _selector = selector;
        _input = input;
    }

    public static IComparer<T> For(Func<T, string?> selector, string? input) =>
        new LevenshteinComparer<T>(selector, input);

    public int Compare(T? first, T? second)
    {
        if (first == null) throw new ArgumentNullException(nameof(first));
        if (second == null) throw new ArgumentNullException(nameof(second));

        var x = _selector(first) ?? string.Empty;
        var y = _selector(second) ?? string.Empty;

        var dist1 = Compare(x, _input ?? string.Empty);
        var dist2 = Compare(y, _input ?? string.Empty);
        return dist1.CompareTo(dist2);
    }

    private static int Compare(string x, string y)
    {
        var n = x.Length;
        var m = y.Length;
        var d = new int[n + 1, m + 1];

        if (n == 0)
        {
            return m;
        }

        if (m == 0)
        {
            return n;
        }

        for (var i = 0; i <= n; d[i, 0] = i++)
        {
        }

        for (var j = 0; j <= m; d[0, j] = j++)
        {
        }

        for (var i = 1; i <= n; i++)
        {
            for (var j = 1; j <= m; j++)
            {
                var cost = y[j - 1] == x[i - 1] ? 0 : 1;

                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }

        return d[n, m];
    }
}