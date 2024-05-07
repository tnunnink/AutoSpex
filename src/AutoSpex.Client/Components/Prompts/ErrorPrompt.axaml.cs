using System;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace AutoSpex.Client.Components;

public class ErrorPrompt : TemplatedControl
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<ErrorPrompt, string?>(
            nameof(Title));

    public static readonly StyledProperty<object?> ErrorContentProperty =
        AvaloniaProperty.Register<ErrorPrompt, object?>(
            nameof(ErrorContent));

    public static readonly StyledProperty<Exception?> ExceptionProperty =
        AvaloniaProperty.Register<ErrorPrompt, Exception?>(
            nameof(Exception));

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public object? ErrorContent
    {
        get => GetValue(ErrorContentProperty);
        set => SetValue(ErrorContentProperty, value);
    }

    public Exception? Exception
    {
        get => GetValue(ExceptionProperty);
        set => SetValue(ExceptionProperty, value);
    }
}