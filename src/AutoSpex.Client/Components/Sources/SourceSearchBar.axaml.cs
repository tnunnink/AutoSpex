using System.Windows.Input;
using AutoSpex.Engine;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;

namespace AutoSpex.Client.Components;

public class SourceSearchBar : TemplatedControl
{
    #region Properties

    public static readonly DirectProperty<SourceSearchBar, Element> ElementProperty =
        AvaloniaProperty.RegisterDirect<SourceSearchBar, Element>(
            nameof(Element), o => o.Element, (o, v) => o.Element = v,
            defaultBindingMode: BindingMode.OneWayToSource);

    public static readonly DirectProperty<SourceSearchBar, string?> FilterTextProperty =
        AvaloniaProperty.RegisterDirect<SourceSearchBar, string?>(
            nameof(FilterText), o => o.FilterText, (o, v) => o.FilterText = v,
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly DirectProperty<SourceSearchBar, ICommand?> SearchCommandProperty =
        AvaloniaProperty.RegisterDirect<SourceSearchBar, ICommand?>(
            nameof(SearchCommand), o => o.SearchCommand, (o, v) => o.SearchCommand = v);

    public static readonly DirectProperty<SourceSearchBar, ICommand?> ClearCommandProperty =
        AvaloniaProperty.RegisterDirect<SourceSearchBar, ICommand?>(
            nameof(ClearCommand), o => o.ClearCommand, (o, v) => o.ClearCommand = v);

    #endregion

    private Element _element = Element.Default;
    private string? _filterText;
    private ICommand? _searchCommand;
    private ICommand? _clearCommand;

    public Element Element
    {
        get => _element;
        set => SetAndRaise(ElementProperty, ref _element, value);
    }

    public string? FilterText
    {
        get => _filterText;
        set => SetAndRaise(FilterTextProperty, ref _filterText, value);
    }

    public ICommand? SearchCommand
    {
        get => _searchCommand;
        set => SetAndRaise(SearchCommandProperty, ref _searchCommand, value);
    }

    public ICommand? ClearCommand
    {
        get => _clearCommand;
        set => SetAndRaise(ClearCommandProperty, ref _clearCommand, value);
    }
}