using AutoSpex.Client.Observers;
using AutoSpex.Client.Pages;
using AutoSpex.Client.Pages.Home;
using AutoSpex.Client.Pages.Projects;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentResults;
using JetBrains.Annotations;

namespace AutoSpex.Client;

[UsedImplicitly]
public partial class ShellViewModel(Navigator navigator) : ObservableRecipient,
    IRecipient<NavigationRequest<HomePageModel>>,
    IRecipient<NavigationRequest<ProjectPageModel>>
{
    [ObservableProperty] private PageViewModel? _currentPage;

    [RelayCommand]
    private void NavigateHome()
    {
        if (CurrentPage is not null && CurrentPage.Route.Equals("HomePageModel")) return;
        navigator.NavigateHome();
    }
    
    [RelayCommand]
    private void NavigateProject(ProjectObserver project)
    {
        if (CurrentPage is not null && CurrentPage.Route.Equals(project.Uri.LocalPath)) return;
        navigator.NavigateProject(project);
    }

    public void Receive(NavigationRequest<HomePageModel> message)
    {
        NavigatePage(message.Page);
        message.Reply(Result.Ok());
    }
    
    public void Receive(NavigationRequest<ProjectPageModel> message)
    {
        NavigatePage(message.Page);
        message.Reply(Result.Ok());
    }

    private void NavigatePage(PageViewModel page)
    {
        if (CurrentPage is not null)
            CurrentPage.IsActive = false;
        
        CurrentPage = page;
    }
}