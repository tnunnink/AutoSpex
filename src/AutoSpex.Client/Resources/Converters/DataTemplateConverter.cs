using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Controls.Templates;
using Avalonia.Data.Converters;

namespace AutoSpex.Client.Resources.Converters;

public class DataTemplateConverter : Dictionary<string, IDataTemplate>, IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string key && TryGetValue(key, out var template))
            return template;
        
        return AvaloniaProperty.UnsetValue;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) 
        => throw new NotSupportedException($"conversion back is not supported by {typeof(DataTemplateConverter)}");
}