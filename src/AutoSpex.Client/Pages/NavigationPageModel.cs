using AutoSpex.Client.Shared;
using CommunityToolkit.Mvvm.ComponentModel;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class NavigationPageModel : PageViewModel
{
    [ObservableProperty] private bool _isSpecsExpanded;
    
}