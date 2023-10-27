using CommunityToolkit.Mvvm.ComponentModel;

namespace L5Spex.Model;

public partial class SpecOptions : ObservableObject
{
    [ObservableProperty]
    private bool _enabled;
    
    [ObservableProperty]
    private bool _required;
    
    [ObservableProperty]
    private bool _cascade;
    
}