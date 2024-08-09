using System;
using Avalonia;

namespace AutoSpex.Client.Resources.Properties;

public class StyleClasses : AvaloniaObject
{
    static StyleClasses()
    {
        ClassesProperty.Changed.AddClassHandler<StyledElement>(HandleClassesChanged);
    }

    public static readonly AttachedProperty<string?> ClassesProperty =
        AvaloniaProperty.RegisterAttached<StyleClasses, StyledElement, string?>("Classes");

    public static string? GetClasses(AvaloniaObject element)
    {
        return element.GetValue(ClassesProperty);
    }

    public static void SetClasses(AvaloniaObject element, string? value)
    {
        element.SetValue(ClassesProperty, value);
    }

    private static void HandleClassesChanged(StyledElement styled, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.NewValue is not string classes) return;
        styled.Classes.Clear();
        var values = classes.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        styled.Classes.AddRange(values);
    }
}