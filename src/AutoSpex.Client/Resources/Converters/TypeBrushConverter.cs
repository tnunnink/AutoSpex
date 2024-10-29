using System;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace AutoSpex.Client.Resources.Converters;

public class TypeBrushConverter : IValueConverter
{
    private const double Alpha = 1;
    private const double Saturation = 0.4;
    private const double Lightness = 0.7;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string type || string.IsNullOrEmpty(type))
            return AvaloniaProperty.UnsetValue;

        var hue = type.Sum(c => c) % 360;
        var color = new HslColor(Alpha, hue, Saturation, Lightness);
        return new SolidColorBrush(color.ToRgb());
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException($"{GetType().Name} does not support ConvertBack.");
    }
}