using AutoSpex.Client.Resources.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;

namespace AutoSpex.Client.Resources.Behaviors;

public class RotateDrawerOnClickBehavior : Behavior<Button>
{
    public static readonly StyledProperty<DrawerViewPlacement> HorizontalPlacementProperty =
        AvaloniaProperty.Register<RotateDrawerOnClickBehavior, DrawerViewPlacement>(
            nameof(HorizontalPlacement), defaultValue: DrawerViewPlacement.Bottom);

    public static readonly StyledProperty<DrawerViewPlacement> VerticalPlacementProperty =
        AvaloniaProperty.Register<RotateDrawerOnClickBehavior, DrawerViewPlacement>(
            nameof(VerticalPlacement), defaultValue: DrawerViewPlacement.Right);

    public DrawerViewPlacement HorizontalPlacement
    {
        get => GetValue(HorizontalPlacementProperty);
        set => SetValue(HorizontalPlacementProperty, value);
    }

    public DrawerViewPlacement VerticalPlacement
    {
        get => GetValue(VerticalPlacementProperty);
        set => SetValue(VerticalPlacementProperty, value);
    }

    protected override void OnAttachedToVisualTree()
    {
        base.OnAttachedToVisualTree();
        AssociatedObject?.AddHandler(Button.ClickEvent, OnClick);
    }

    protected override void OnDetachedFromVisualTree()
    {
        base.OnDetachedFromVisualTree();
        AssociatedObject?.RemoveHandler(Button.ClickEvent, OnClick);
    }

    private void OnClick(object? sender, RoutedEventArgs e)
    {
        if (e.Source is not Control control) return;

        var drawer = control.FindAncestorOfType<DrawerView>();
        if (drawer is null) return;

        var placement = drawer.DrawerPlacement == HorizontalPlacement ? VerticalPlacement : HorizontalPlacement;
        drawer.DrawerPlacement = placement;
    }
}