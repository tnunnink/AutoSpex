using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoSpex.Client.Pages;

public partial class RepoSelectorPageModel : PageViewModel
{
    [ObservableProperty] private RepoObserver? _repo;
    
    
}