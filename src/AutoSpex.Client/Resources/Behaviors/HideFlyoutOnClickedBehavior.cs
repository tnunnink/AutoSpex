using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;

namespace AutoSpex.Client.Resources.Behaviors;

/// <summary>
/// Hides the containing popup of a button control when clicked after executing the command. This is a copy of the
/// default Avalonia.Behaviors one but for a Button and not a RadioButton, and I'm just using simple even handlers
/// because the reactive extensions was throwing exceptions.
/// </summary>
public class HideFlyoutOnClickedBehavior : Behavior<Button>
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

    private void OnClick(object? sender, RoutedEventArgs e)
    {
        var flyout = AssociatedObject?.FindAncestorOfType<FlyoutPresenter>();
        if (flyout?.Parent is not Popup popup) return;

        // Execute Command if any before closing.
        // Otherwise, it won't execute because Close will destroy the associated object before Click can execute it.
        if (AssociatedObject?.Command is not null && AssociatedObject.IsEnabled)
        {
            AssociatedObject.Command.Execute(AssociatedObject.CommandParameter);
        }

        popup.Close();
    }
}