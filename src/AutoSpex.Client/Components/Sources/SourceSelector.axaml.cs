using System.Collections.ObjectModel;
using AutoSpex.Client.Observers;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace AutoSpex.Client.Components;

public class SourceSelector : TemplatedControl
{
    #region AvaloniaProperties
    
    public static readonly DirectProperty<SourceSelector, ObservableCollection<SourceObserver>> SourcesProperty =
        AvaloniaProperty.RegisterDirect<SourceSelector, ObservableCollection<SourceObserver>>(
            nameof(Sources), o => o.Sources, (o, v) => o.Sources = v);

    public static readonly DirectProperty<SourceSelector, SourceObserver?> SelectedSourceProperty =
        AvaloniaProperty.RegisterDirect<SourceSelector, SourceObserver?>(
            nameof(SelectedSource), o => o.SelectedSource, (o, v) => o.SelectedSource = v);
    
    #endregion
    
    private ObservableCollection<SourceObserver> _sources = [];
    private SourceObserver? _selectedSource;
    
    public ObservableCollection<SourceObserver> Sources
    {
        get => _sources;
        set => SetAndRaise(SourcesProperty, ref _sources, value);
    }
    
    public SourceObserver? SelectedSource
    {
        get => _selectedSource;
        set => SetAndRaise(SelectedSourceProperty, ref _selectedSource, value);
    }
}