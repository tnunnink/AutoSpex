using AutoSpex.Client.Features.Projects;
using AutoSpex.Client.Shared;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;
using MediatR;
using Project = AutoSpex.Client.Features.Projects.Project;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class AppPageModel : ViewModelBase, IRecipient<LaunchProjectMessage>
{
    public AppPageModel(IMessenger messenger, IMediator mediator)
    {
        messenger.RegisterAll(this);
        /*WindowHeight = 800;
        WindowWidth = 800;*/
        CurrentPage = new LauncherView();
    }

    [ObservableProperty] private Control _currentPage;

    [ObservableProperty] private Project? _project;

    public void Receive(LaunchProjectMessage message)
    {
        Project = message.Project;
        CurrentPage = new ProjectPage();
    }
}