using AutoSpex.Client.Observers;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentResults;
using JetBrains.Annotations;

namespace AutoSpex.Client;

[UsedImplicitly]
public partial class ShellViewModel : ObservableRecipient, IRecipient<NavigationRequest>
{
    private readonly Navigator _navigator;

    public ShellViewModel(Navigator navigator)
    {
        _navigator = navigator;
        Messenger.Register<NavigationRequest, string>(this, nameof(ShellViewModel));
    }

    [ObservableProperty] private PageViewModel? _currentPage;

    [RelayCommand]
    private void NavigateHome()
    {
        if (CurrentPage is not null && CurrentPage.Route.Equals("HomePageModel")) return;
        _navigator.NavigateHome();
    }
    
    [RelayCommand]
    private void NavigateProject(ProjectObserver project)
    {
        if (CurrentPage is not null && CurrentPage.Route.Equals(project.Uri.LocalPath)) return;
        _navigator.NavigateProject(project);
    }

    public void Receive(NavigationRequest message)
    {
        CurrentPage = message.Page;
        message.Reply(Result.Ok());
    }
}