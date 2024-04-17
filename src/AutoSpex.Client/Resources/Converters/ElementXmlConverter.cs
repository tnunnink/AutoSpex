using System;
using System.Globalization;
using System.Xml.Linq;
using Avalonia;
using Avalonia.Data.Converters;
using L5Sharp.Core;

namespace AutoSpex.Client.Resources.Converters;

public class ElementXmlConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ILogixSerializable serializable)
        {
            return serializable.Serialize().ToString();
        }

        return AvaloniaProperty.UnsetValue;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string xml) return default;
        var element = new XElement(xml);

        if (targetType == typeof(LogixElement))
        {
            return element.Deserialize();
        }

        return default;
    }
}