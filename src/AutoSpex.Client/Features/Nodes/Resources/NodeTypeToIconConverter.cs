using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace AutoSpex.Client.Features.Nodes;

public class NodeTypeToIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var nodeType = value as NodeType;
        
        return nodeType?.Name switch
        {
            nameof(NodeType.Collection) => Application.Current?.Resources["Icon.Outlined.Folder"],
            nameof(NodeType.Folder) => Application.Current?.Resources["Icon.Outlined.Folder"],
            nameof(NodeType.Spec) => Application.Current?.Resources["Icon.Outlined.Clipboard"],
            nameof(NodeType.Source) => Application.Current?.Resources["Icon.Xml"],
            _ => AvaloniaProperty.UnsetValue
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}