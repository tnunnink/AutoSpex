using Avalonia;
using Avalonia.Controls.Primitives;

namespace AutoSpex.Client.Resources.Controls;

public class MessageBox : TemplatedControl
{
    #region Properties

    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<MessageBox, string?>(
            nameof(Title));

    public static readonly StyledProperty<string> MessageProperty =
        AvaloniaProperty.Register<MessageBox, string>(
            nameof(Message));

    public static readonly StyledProperty<MessageBoxStatus?> StatusProperty =
        AvaloniaProperty.Register<MessageBox, MessageBoxStatus?>(
            nameof(Status));

    #endregion


    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public MessageBoxStatus? Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }
}

public enum MessageBoxStatus
{
    Information,
    Question,
    Warning,
    Error
}

public enum MessageAnswer
{
    Ok,
    Cancel,
    Yes,
    No
}