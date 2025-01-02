using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;

namespace AutoSpex.Client.Resources.Behaviors;

public class HideFlyoutPointerReleasedBehavior : Behavior<InputElement>
{
    private RoutingStrategies RoutingStrategies { get; set; } =
        RoutingStrategies.Tunnel | RoutingStrategies.Direct | RoutingStrategies.Bubble;


    protected override void OnAttachedToVisualTree()
    {
        base.OnAttachedToVisualTree();
        AssociatedObject?.AddHandler(InputElement.PointerReleasedEvent, OnRelease, RoutingStrategies);
    }

    protected override void OnDetachedFromVisualTree()
    {
        base.OnDetachedFromVisualTree();
        AssociatedObject?.RemoveHandler(InputElement.PointerReleasedEvent, OnRelease);
    }

    private void OnRelease(object? sender, RoutedEventArgs e)
    {
        var flyout = AssociatedObject?.FindAncestorOfType<FlyoutPresenter>();
        if (flyout?.Parent is not Popup popup) return;
        popup.Close();
    }
}