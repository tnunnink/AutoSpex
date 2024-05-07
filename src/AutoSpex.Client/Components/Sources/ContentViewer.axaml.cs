using System.Collections.ObjectModel;
using AutoSpex.Client.Observers;
using AutoSpex.Engine;
using Avalonia;
using Avalonia.Controls.Primitives;
using L5Sharp.Core;

namespace AutoSpex.Client.Components;

public class ContentViewer : TemplatedControl
{
    public static readonly DirectProperty<ContentViewer, SourceObserver?> SourceProperty =
        AvaloniaProperty.RegisterDirect<ContentViewer, SourceObserver?>(
            nameof(Source), o => o.Source, (o, v) => o.Source = v);

    private SourceObserver? _source;

    public SourceObserver? Source
    {
        get => _source;
        set => SetAndRaise(SourceProperty, ref _source, value);
    }
    
    public Element Element { get; set; } = Element.Default;
    public bool Searching { get; private set; }
    public ObservableCollection<LogixElement> Elements { get; } = [];
}