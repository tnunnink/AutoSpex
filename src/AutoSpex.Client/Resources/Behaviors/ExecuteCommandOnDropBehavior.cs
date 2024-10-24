using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Xaml.Interactivity;

namespace AutoSpex.Client.Resources.Behaviors;

public class ExecuteCommandOnDropBehavior : Behavior<Control>
{
    private const string DragOverClass = "dragover";

    public static readonly StyledProperty<ICommand?> CommandProperty =
        AvaloniaProperty.Register<ExecuteCommandOnDropBehavior, ICommand?>(
            nameof(Command));

    public static readonly StyledProperty<string> DataFormatProperty =
        AvaloniaProperty.Register<ExecuteCommandOnDropBehavior, string>(
            nameof(DataFormat), defaultValue: "Context");

    public static readonly StyledProperty<StyledElement?> StyleElementProperty =
        AvaloniaProperty.Register<ExecuteCommandOnDropBehavior, StyledElement?>(
            nameof(StyleElement));

    /// <summary>
    /// The command to execute when the drop event is triggered. This command should accept the source control
    /// data context as the parameter. This way we can also use the CanExecute() of the command to determine if the
    /// source is valid and update the dragover class of the target control.
    /// </summary>
    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>
    /// The data format of the expected drag event args object. This can be used to get the specific data from
    /// the <see cref="DragEventArgs"/>. The default is "Context". 
    /// </summary>
    public string DataFormat
    {
        get => GetValue(DataFormatProperty);
        set => SetValue(DataFormatProperty, value);
    }

    /// <summary>
    /// The element to apply the styled "dragover" class to when the drag over is active. If not specified this will
    /// default to the AssociatedObject
    /// </summary>
    public StyledElement? StyleElement
    {
        get => GetValue(StyleElementProperty);
        set => SetValue(StyleElementProperty, value);
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree()
    {
        if (AssociatedObject is not null)
        {
            DragDrop.SetAllowDrop(AssociatedObject, true);
        }

        AssociatedObject?.AddHandler(DragDrop.DragEnterEvent, DragEnter);
        AssociatedObject?.AddHandler(DragDrop.DragLeaveEvent, DragLeave);
        AssociatedObject?.AddHandler(DragDrop.DragOverEvent, DragOver);
        AssociatedObject?.AddHandler(DragDrop.DropEvent, Drop);
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree()
    {
        if (AssociatedObject is not null)
        {
            DragDrop.SetAllowDrop(AssociatedObject, false);
        }

        AssociatedObject?.RemoveHandler(DragDrop.DragEnterEvent, DragEnter);
        AssociatedObject?.RemoveHandler(DragDrop.DragLeaveEvent, DragLeave);
        AssociatedObject?.RemoveHandler(DragDrop.DragOverEvent, DragOver);
        AssociatedObject?.RemoveHandler(DragDrop.DropEvent, Drop);
    }

    private void DragEnter(object? sender, DragEventArgs e)
    {
        var source = e.Data.Get(DataFormat);

        if (Command?.CanExecute(source) is not true)
        {
            e.DragEffects = DragDropEffects.None;
            return;
        }

        UpdateStyledElement(true);
    }

    private void DragLeave(object? sender, DragEventArgs e)
    {
        UpdateStyledElement(false);
    }

    private void DragOver(object? sender, DragEventArgs e)
    {
        var source = e.Data.Get(DataFormat);

        if (Command?.CanExecute(source) is not true)
        {
            e.DragEffects = DragDropEffects.None;
            return;
        }

        UpdateStyledElement(true);
    }

    private void Drop(object? sender, DragEventArgs e)
    {
        var source = e.Data.Get(DataFormat);

        if (Command?.CanExecute(source) is true)
        {
            Command.Execute(source);
        }

        UpdateStyledElement(false);
    }

    private void UpdateStyledElement(bool isDragover)
    {
        var element = StyleElement ?? AssociatedObject;
        if (element is null) return;

        switch (isDragover)
        {
            case true when !element.Classes.Contains(DragOverClass):
                element.Classes.Add(DragOverClass);
                break;
            case false when element.Classes.Contains(DragOverClass):
                element.Classes.Remove(DragOverClass);
                break;
        }
    }
}