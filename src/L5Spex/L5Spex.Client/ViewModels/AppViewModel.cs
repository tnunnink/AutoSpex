using CommunityToolkit.Mvvm.ComponentModel;

namespace L5Spex.Client.ViewModels;

public partial class AppViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isSidebarOpen = true;
}