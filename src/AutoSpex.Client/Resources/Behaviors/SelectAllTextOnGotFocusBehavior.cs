using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.Xaml.Interactivity;

namespace AutoSpex.Client.Resources.Behaviors;

public class SelectAllTextOnGotFocusBehavior : StyledElementBehavior<TextBox>
{
    /// <inheritdoc />
    protected override void OnAttachedToVisualTree()
    {
        AssociatedObject?.AddHandler(InputElement.GotFocusEvent, AssociatedObject_GotFocus, RoutingStrategies.Bubble);
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree()
    {
        AssociatedObject?.RemoveHandler(InputElement.GotFocusEvent, AssociatedObject_GotFocus);
    }

    private void AssociatedObject_GotFocus(object? sender, GotFocusEventArgs e)
    {
        Dispatcher.UIThread.Post(() => { AssociatedObject?.SelectAll(); });
    }
}