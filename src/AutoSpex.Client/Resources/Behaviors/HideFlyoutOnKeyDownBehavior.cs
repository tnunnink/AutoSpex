using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;

namespace AutoSpex.Client.Resources.Behaviors;

public class HideFlyoutOnKeyDownBehavior : Behavior<Control>
{
    public static readonly StyledProperty<Key> KeyProperty =
        AvaloniaProperty.Register<HideFlyoutOnKeyDownBehavior, Key>(nameof(Key), Key.Enter);

    public Key Key
    {
        get => GetValue(KeyProperty);
        set => SetValue(KeyProperty, value);
    }

    protected override void OnAttachedToVisualTree()
    {
        base.OnAttachedToVisualTree();
        AssociatedObject?.AddHandler(InputElement.KeyDownEvent, OnKeyDown);
    }

    protected override void OnDetachedFromVisualTree()
    {
        base.OnDetachedFromVisualTree();
        AssociatedObject?.RemoveHandler(InputElement.KeyDownEvent, OnKeyDown);
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key) return;

        var flyout = AssociatedObject?.FindAncestorOfType<FlyoutPresenter>();
        if (flyout?.Parent is not Popup popup) return;

        popup.Close();
    }
}