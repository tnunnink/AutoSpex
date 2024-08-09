using AutoSpex.Client.Resources.Controls;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;

namespace AutoSpex.Client.Resources.Behaviors;

public class ToggleDrawerOnClickBehavior : Behavior<Button>
{
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

    private static void OnClick(object? sender, RoutedEventArgs e)
    {
        if (e.Source is not Control control) return;
        var drawer = control.FindAncestorOfType<DrawerView>();
        if (drawer is null) return;
        drawer.IsDrawerOpen = !drawer.IsDrawerOpen;
    }
}