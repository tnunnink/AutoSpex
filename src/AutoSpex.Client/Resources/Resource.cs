using Avalonia;
using Avalonia.Controls;

namespace AutoSpex.Client.Resources;

public static class Resource
{
    public static object? Find(string? key)
    {
        return !string.IsNullOrEmpty(key)
            ? Application.Current?.FindResource(key)
            : AvaloniaProperty.UnsetValue;
    }
}