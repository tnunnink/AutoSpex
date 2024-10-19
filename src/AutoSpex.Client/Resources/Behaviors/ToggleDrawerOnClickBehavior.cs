using AutoSpex.Client.Resources.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;

namespace AutoSpex.Client.Resources.Behaviors;

public class ToggleDrawerOnClickBehavior : Behavior<Button>
{
    public static readonly StyledProperty<bool> AlwaysOpenProperty =
        AvaloniaProperty.Register<ToggleDrawerOnClickBehavior, bool>(
            nameof(AlwaysOpen));

    public bool AlwaysOpen
    {
        get => GetValue(AlwaysOpenProperty);
        set => SetValue(AlwaysOpenProperty, value);
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
        drawer.IsDrawerOpen = AlwaysOpen || !drawer.IsDrawerOpen;
    }
}