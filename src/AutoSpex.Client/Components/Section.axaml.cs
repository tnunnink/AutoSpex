using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace AutoSpex.Client.Components;

public class Section : HeaderedContentControl
{
    #region Properties

    public static readonly StyledProperty<IBrush?> HeaderBackgroundProperty =
        AvaloniaProperty.Register<Section, IBrush?>(
            nameof(HeaderBackground));

    public static readonly StyledProperty<Thickness> HeaderPaddingProperty =
        AvaloniaProperty.Register<Section, Thickness>(
            nameof(HeaderPadding));

    public static readonly StyledProperty<int> HeaderMaxHeightProperty =
        AvaloniaProperty.Register<Section, int>(
            nameof(HeaderMaxHeight));

    public static readonly StyledProperty<int> HeaderMinHeightProperty =
        AvaloniaProperty.Register<Section, int>(
            nameof(HeaderMinHeight));

    #endregion

    public IBrush? HeaderBackground
    {
        get => GetValue(HeaderBackgroundProperty);
        set => SetValue(HeaderBackgroundProperty, value);
    }

    public Thickness HeaderPadding
    {
        get => GetValue(HeaderPaddingProperty);
        set => SetValue(HeaderPaddingProperty, value);
    }

    public int HeaderMinHeight
    {
        get => GetValue(HeaderMinHeightProperty);
        set => SetValue(HeaderMinHeightProperty, value);
    }

    public int HeaderMaxHeight
    {
        get => GetValue(HeaderMaxHeightProperty);
        set => SetValue(HeaderMaxHeightProperty, value);
    }
}