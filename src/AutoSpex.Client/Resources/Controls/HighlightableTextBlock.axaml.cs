using System;
using Avalonia;
using Avalonia.Controls;

namespace AutoSpex.Client.Resources.Controls;

public class HighlightableTextBlock : SelectableTextBlock
{
    public static readonly StyledProperty<string?> HighlightedTextProperty =
        AvaloniaProperty.Register<HighlightableTextBlock, string?>(nameof(HighlightedText));

    public string? HighlightedText
    {
        get => GetValue(HighlightedTextProperty);
        set => SetValue(HighlightedTextProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == HighlightedTextProperty)
            UpdateTextHighlight(change.GetNewValue<string?>());
    }

    private void UpdateTextHighlight(string? text)
    {
        SelectionStart = 0;
        SelectionEnd = 0;
        
        if (text is null || Text is null) return; 
        
        var textLower = Text.ToLower();
        var highlightedTextLower = text.ToLower();

        var index = textLower.IndexOf(highlightedTextLower, StringComparison.Ordinal);

        if (index == -1) return;
        SelectionStart = index;
        SelectionEnd = index + text.Length;
    }
}