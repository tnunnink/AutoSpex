using System.Collections.ObjectModel;
using AutoSpex.Client.Observers;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace AutoSpex.Client.Components;

public class SpecList : TemplatedControl
{

    public static readonly DirectProperty<SpecList, ObservableCollection<NodeObserver>> SpecsProperty =
        AvaloniaProperty.RegisterDirect<SpecList, ObservableCollection<NodeObserver>>(
            nameof(Specs), o => o.Specs, (o, v) => o.Specs = v);

    private ObservableCollection<NodeObserver> _specs = [];

    public ObservableCollection<NodeObserver> Specs
    {
        get => _specs;
        set => SetAndRaise(SpecsProperty, ref _specs, value);
    }
}