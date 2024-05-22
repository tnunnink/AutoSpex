using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.LogicalTree;
using Avalonia.Metadata;

namespace AutoSpex.Client.Resources.Controls;

/// <summary>
/// A control with two views: A collapsible pane and an area for content
/// </summary>
[TemplatePart("PART_PaneRoot", typeof(Panel))]
[PseudoClasses(ClassOpen, ClassClosed)]
[PseudoClasses(ClassLeft, ClassRight, ClassTop, ClassBottom)]
public class DrawerView : ContentControl
{
    private const string ClassOpen = ":open";
    private const string ClassClosed = ":closed";
    private const string ClassLeft = ":left";
    private const string ClassRight = ":right";
    private const string ClassTop = ":top";
    private const string ClassBottom = ":bottom";
    private string? _lastPlacement;
    private Panel? _drawer;
    private GridLength _drawerGridLength;

    public static readonly StyledProperty<bool> IsDrawerOpenProperty =
        AvaloniaProperty.Register<DrawerView, bool>(
            nameof(IsDrawerOpen),
            defaultValue: false,
            coerce: CoerceIsPaneOpen);

    public static readonly StyledProperty<double> DrawerClosedLengthProperty =
        AvaloniaProperty.Register<DrawerView, double>(
            nameof(DrawerClosedLength),
            defaultValue: 30);

    public static readonly StyledProperty<double> DrawerMaxLengthProperty =
        AvaloniaProperty.Register<DrawerView, double>(
            nameof(DrawerMaxLength),
            defaultValue: 1000);

    public static readonly StyledProperty<double> DrawerOpenLengthProperty =
        AvaloniaProperty.Register<DrawerView, double>(
            nameof(DrawerOpenLength),
            defaultValue: 300);

    public static readonly DirectProperty<DrawerView, GridLength> DrawerGridLengthProperty =
        AvaloniaProperty.RegisterDirect<DrawerView, GridLength>(
            nameof(DrawerGridLength), o => o.DrawerGridLength);

    public static readonly StyledProperty<DrawerViewPlacement> DrawerPlacementProperty =
        AvaloniaProperty.Register<DrawerView, DrawerViewPlacement>(nameof(DrawerPlacement),
            defaultValue: DrawerViewPlacement.Left);

    public static readonly StyledProperty<object?> DrawerProperty =
        AvaloniaProperty.Register<DrawerView, object?>(nameof(Drawer));

    public static readonly StyledProperty<IDataTemplate> DrawerTemplateProperty =
        AvaloniaProperty.Register<DrawerView, IDataTemplate>(nameof(DrawerTemplate));

    /// <summary>
    /// A bit that indicates and controls whether the drawer panel is open or closed.
    /// </summary>
    public bool IsDrawerOpen
    {
        get => GetValue(IsDrawerOpenProperty);
        set => SetValue(IsDrawerOpenProperty, value);
    }

    /// <summary>
    /// The default length (height or width) of the drawer panel when the drawer is opened.
    /// This is only th initial open length. Once the grid is moved by the user it will remain at that position until
    /// the control is re-rendered.
    /// </summary>
    public double DrawerOpenLength
    {
        get => GetValue(DrawerOpenLengthProperty);
        set => SetValue(DrawerOpenLengthProperty, value);
    }

    /// <summary>
    /// The length (height or width) of the drawer panel when the drawer is closed.
    /// </summary>
    public double DrawerClosedLength
    {
        get => GetValue(DrawerClosedLengthProperty);
        set => SetValue(DrawerClosedLengthProperty, value);
    }

    /// <summary>
    /// A maximum length (height or width) of the drawer panel when open.
    /// </summary>
    public double DrawerMaxLength
    {
        get => GetValue(DrawerMaxLengthProperty);
        set => SetValue(DrawerMaxLengthProperty, value);
    }

    /// <summary>
    /// The actual computed grid length of the drawer panel.
    /// </summary>
    public GridLength DrawerGridLength
    {
        get => _drawerGridLength;
        set => SetAndRaise(DrawerGridLengthProperty, ref _drawerGridLength, value);
    }

    /// <summary>
    /// Where the expandable drawer panel is placed in the control (left, top, right, bottom).
    /// </summary>
    public DrawerViewPlacement DrawerPlacement
    {
        get => GetValue(DrawerPlacementProperty);
        set => SetValue(DrawerPlacementProperty, value);
    }

    /// <summary>
    /// Gets or sets the Drawer for the SplitView
    /// </summary>
    [DependsOn(nameof(DrawerTemplate))]
    public object? Drawer
    {
        get => GetValue(DrawerProperty);
        set => SetValue(DrawerProperty, value);
    }

    /// <summary>
    /// Gets or sets the data template used to display the header content of the control.
    /// </summary>
    public IDataTemplate DrawerTemplate
    {
        get => GetValue(DrawerTemplateProperty);
        set => SetValue(DrawerTemplateProperty, value);
    }

    protected override bool RegisterContentPresenter(ContentPresenter presenter)
    {
        var result = base.RegisterContentPresenter(presenter);

        if (presenter.Name == "PART_DrawerPresenter")
        {
            return true;
        }

        return result;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        RegisterDrawerPart(e);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        // left and right style triggers contain the template, so we need to do this as
        // soon as we're attached so the template applies. The other visual states can
        // be updated after the template applies
        UpdateVisualStateForPanePlacementProperty(DrawerPlacement);
        UpdateVisualStateForDrawerOpen(IsDrawerOpen);
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsDrawerOpenProperty)
            UpdateVisualStateForDrawerOpen(change.GetNewValue<bool>());

        if (change.Property == DrawerProperty)
            UpdateVisualStateForDrawer(change);

        if (change.Property == DrawerPlacementProperty)
            UpdateVisualStateForPanePlacementProperty(change.GetNewValue<DrawerViewPlacement>());
    }

    private void RegisterDrawerPart(TemplateAppliedEventArgs e)
    {
        if (_drawer is not null) _drawer.SizeChanged -= DrawerSizeChanged;
        _drawer = e.NameScope.Find<Panel>("PART_DrawerRoot");
        if (_drawer is null) return;
        _drawer.SizeChanged += DrawerSizeChanged;
    }

    private void DrawerSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (!IsDrawerOpen) return;

        if (DrawerPlacement is DrawerViewPlacement.Left or DrawerViewPlacement.Right)
        {
            DrawerOpenLength = e.NewSize.Width;
        }

        if (DrawerPlacement is DrawerViewPlacement.Top or DrawerViewPlacement.Bottom)
        {
            DrawerOpenLength = e.NewSize.Height;
        }
    }

    private void UpdateVisualStateForDrawerOpen(bool isOpen)
    {
        if (isOpen)
        {
            DrawerGridLength = new GridLength(DrawerOpenLength, GridUnitType.Pixel);
            PseudoClasses.Add(ClassOpen);
            PseudoClasses.Remove(ClassClosed);
        }
        else
        {
            DrawerGridLength = new GridLength(DrawerClosedLength, GridUnitType.Pixel);
            PseudoClasses.Add(ClassClosed);
            PseudoClasses.Remove(ClassOpen);
        }
    }

    private void UpdateVisualStateForDrawer(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.OldValue is ILogical oldChild)
        {
            LogicalChildren.Remove(oldChild);
        }

        if (change.NewValue is ILogical newChild)
        {
            LogicalChildren.Add(newChild);
        }
    }

    private void UpdateVisualStateForPanePlacementProperty(DrawerViewPlacement newValue)
    {
        if (!string.IsNullOrEmpty(_lastPlacement)) PseudoClasses.Remove(_lastPlacement);
        _lastPlacement = GetPseudoClass(newValue);
        PseudoClasses.Add(_lastPlacement);
    }

    private static bool CoerceIsPaneOpen(AvaloniaObject instance, bool value) => value;

    /// <summary>
    /// Gets the appropriate PseudoClass for the given <see cref="SplitViewPanePlacement"/>.
    /// </summary>
    private static string GetPseudoClass(DrawerViewPlacement placement)
    {
        return placement switch
        {
            DrawerViewPlacement.Left => ClassLeft,
            DrawerViewPlacement.Right => ClassRight,
            DrawerViewPlacement.Top => ClassTop,
            DrawerViewPlacement.Bottom => ClassBottom,
            _ => throw new ArgumentOutOfRangeException(nameof(placement), placement, null)
        };
    }
}