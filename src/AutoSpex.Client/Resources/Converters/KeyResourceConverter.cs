using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Metadata;

namespace AutoSpex.Client.Resources.Converters;

public class ValueResourceKey
{
    public object? Value { get; set; }
    public string Resource { get; set; } = string.Empty;
}

public class KeyResourceConverter : IValueConverter
{
    [Content]
    // ReSharper disable once CollectionNeverUpdated.Global updated in resource files
    public List<ValueResourceKey> Map { get; set; } = [];

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var entry = Map.FirstOrDefault(x => Equals(value, x.Value));
        return entry != null  
            ? Application.Current?.FindResource(entry.Resource)
            : AvaloniaProperty.UnsetValue;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException($"{GetType().Name} does not support ConvertBack.");
    }
}

