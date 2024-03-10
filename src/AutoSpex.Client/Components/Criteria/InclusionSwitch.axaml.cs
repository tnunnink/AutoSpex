using System.Windows.Input;
using AutoSpex.Engine;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using CommunityToolkit.Mvvm.Input;

namespace AutoSpex.Client.Components;

public class InclusionSwitch : TemplatedControl
{

    public static readonly DirectProperty<InclusionSwitch, Inclusion> InclusionProperty =
        AvaloniaProperty.RegisterDirect<InclusionSwitch, Inclusion>(
            nameof(Inclusion), o => o.Inclusion, (o, v) => o.Inclusion = v, 
            defaultBindingMode: BindingMode.TwoWay,
            unsetValue: Inclusion.All);

    private Inclusion _inclusion;

    public InclusionSwitch()
    {
        SetInclusionCommand = new RelayCommand<Inclusion>(SetInclusion);
    }
    
    public Inclusion Inclusion
    {
        get => _inclusion;
        set => SetAndRaise(InclusionProperty, ref _inclusion, value);
    }
    
    public ICommand SetInclusionCommand { get; }


    private void SetInclusion(Inclusion inclusion)
    {
        Inclusion = inclusion;
    }
}