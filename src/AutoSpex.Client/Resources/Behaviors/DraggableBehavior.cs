using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Xaml.Interactions.DragAndDrop;
using Avalonia.Xaml.Interactivity;

namespace AutoSpex.Client.Resources.Behaviors;

public class DraggableBehavior : Behavior<Control>
{
    private PointerEventArgs? _triggerEvent;

    public static readonly StyledProperty<object?> ContextProperty =
        AvaloniaProperty.Register<DraggableBehavior, object?>(nameof(Context));

    public static readonly StyledProperty<string> DataFormatProperty =
        AvaloniaProperty.Register<DraggableBehavior, string>(nameof(DataFormat), "Context");

    public static readonly StyledProperty<double> HorizontalDragThresholdProperty =
        AvaloniaProperty.Register<ContextDragBehavior, double>(nameof(HorizontalDragThreshold), 5);

    public static readonly StyledProperty<double> VerticalDragThresholdProperty =
        AvaloniaProperty.Register<ContextDragBehavior, double>(nameof(VerticalDragThreshold), 5);

    public static readonly StyledProperty<Visual?> GhostProperty =
        AvaloniaProperty.Register<DraggableBehavior, Visual?>(
            nameof(Ghost));

    /// <summary>
    /// The optional data context binding to use as the data object for the draggable item.
    /// </summary>
    /// <remarks>
    /// If not set then this behavior uses the <c>AssociatedObject</c> DataContext property as the data object.
    /// </remarks>>
    public object? Context
    {
        get => GetValue(ContextProperty);
        set => SetValue(ContextProperty, value);
    }

    /// <summary>
    /// Gets or sets the data format for the DraggableBehavior.
    /// </summary>
    /// <remarks>
    /// The data format determines the format of the data that can be dragged and dropped using the DraggableBehavior.
    /// The default value is "Context".
    /// </remarks>
    public string DataFormat
    {
        get => GetValue(DataFormatProperty);
        set => SetValue(DataFormatProperty, value);
    }

    /// <summary>
    /// Gets or sets the horizontal drag threshold for the DraggableBehavior.
    /// </summary>
    /// <remarks>
    /// The horizontal drag threshold determines the minimum distance the pointer must be moved horizontally in order for a drag operation to be initiated.
    /// If the pointer is moved less than the horizontal drag threshold, the drag operation will not start.
    /// The default value is 5.
    /// </remarks>
    public double HorizontalDragThreshold
    {
        get => GetValue(HorizontalDragThresholdProperty);
        set => SetValue(HorizontalDragThresholdProperty, value);
    }

    /// <summary>
    /// Gets or sets the vertical drag threshold for the DraggableBehavior.
    /// </summary>
    /// <remarks>
    /// The vertical drag threshold determines the minimum distance the pointer must be moved vertically in order for a drag operation to be initiated.
    /// If the pointer is moved less than the vertical drag threshold, the drag operation will not start.
    /// The default value is 5.
    /// </remarks>
    public double VerticalDragThreshold
    {
        get => GetValue(VerticalDragThresholdProperty);
        set => SetValue(VerticalDragThresholdProperty, value);
    }

    public Visual? Ghost
    {
        get => GetValue(GhostProperty);
        set => SetValue(GhostProperty, value);
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree()
    {
        const RoutingStrategies strategies =
            RoutingStrategies.Direct | RoutingStrategies.Tunnel | RoutingStrategies.Bubble;

        AssociatedObject?.AddHandler(InputElement.PointerPressedEvent, PointerPressed, strategies);
        AssociatedObject?.AddHandler(InputElement.PointerReleasedEvent, PointerReleased, strategies);
        AssociatedObject?.AddHandler(InputElement.PointerMovedEvent, PointerMoved, strategies);
        AssociatedObject?.AddHandler(InputElement.PointerCaptureLostEvent, CaptureLost, strategies);
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree()
    {
        AssociatedObject?.RemoveHandler(InputElement.PointerPressedEvent, PointerPressed);
        AssociatedObject?.RemoveHandler(InputElement.PointerReleasedEvent, PointerReleased);
        AssociatedObject?.RemoveHandler(InputElement.PointerMovedEvent, PointerMoved);
        AssociatedObject?.RemoveHandler(InputElement.PointerCaptureLostEvent, CaptureLost);
    }

    private void PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var properties = e.GetCurrentPoint(AssociatedObject).Properties;
        if (!properties.IsLeftButtonPressed) return;
        _triggerEvent = e;
    }

    /// <summary>
    /// Perform the drag drop when the trigger even is not null and the pointer has moved outside the threshold.
    /// </summary>
    private async void PointerMoved(object? sender, PointerEventArgs e)
    {
        //Check if properly triggered
        var properties = e.GetCurrentPoint(AssociatedObject).Properties;
        if (!properties.IsLeftButtonPressed || _triggerEvent is null) return;

        //Check if current point is outside the drag threshold.
        var startPoint = _triggerEvent.GetPosition(null);
        var currentPoint = e.GetPosition(null);
        var delta = startPoint - currentPoint;
        if (Math.Abs(delta.X) < HorizontalDragThreshold && Math.Abs(delta.Y) < VerticalDragThreshold) return;

        if (Ghost is not null)
        {
            Ghost.RenderTransform = new TranslateTransform(delta.X, delta.Y);
            Ghost.IsVisible = true;
        }

        //Perform drag drop
        var context = Context ?? AssociatedObject?.DataContext;
        await DoDragDrop(_triggerEvent, context);

        _triggerEvent = null;
    }

    /// <summary>
    /// Reset the trigger event args when the pointer is released.
    /// </summary>
    private void PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _triggerEvent = null;

        if (Ghost is not null)
        {
            Ghost.IsVisible = false;
        }
    }

    /// <summary>
    /// Reset the trigger event args when the pointer is capture is lost.
    /// </summary>
    private void CaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        _triggerEvent = null;

        if (Ghost is not null)
        {
            Ghost.IsVisible = false;
        }
    }

    /// <summary>
    /// Execute the drag drop using the provided trigger event and data context.
    /// </summary>
    private async Task DoDragDrop(PointerEventArgs triggerEvent, object? value)
    {
        var data = new DataObject();
        data.Set(DataFormat, value!);

        var effect = DragDropEffects.None;

        if (triggerEvent.KeyModifiers.HasFlag(KeyModifiers.Alt))
        {
            effect |= DragDropEffects.Link;
        }
        else if (triggerEvent.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            effect |= DragDropEffects.Move;
        }
        else if (triggerEvent.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            effect |= DragDropEffects.Copy;
        }
        else
        {
            effect |= DragDropEffects.Move;
        }

        await DragDrop.DoDragDrop(triggerEvent, data, effect);
    }
}