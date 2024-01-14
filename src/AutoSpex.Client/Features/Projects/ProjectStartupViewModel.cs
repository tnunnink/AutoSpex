using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HanumanInstitute.MvvmDialogs;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Client.Features;

[UsedImplicitly]
public partial class ProjectStartupViewModel(IDialogService dialog) : ViewModelBase
{
    [RelayCommand]
    private async Task NewProject()
    {
        var path = await dialog.ShowNewProjectDialog();
        if (path is null) return;

        var project = new Project(path);

        var result = await Mediator.Send(new ProjectRequest.Create(project));

        if (result.IsSuccess)
        {
            Messenger.Send(new ProjectLaunchedMessage(project));
        }
        
        //todo it should notify if failed, but should we do something else here? Or just let them try again?
    }

    [RelayCommand]
    private async Task OpenProject()
    {
        var path = await dialog.ShowSelectProjectDialog("Select Spex Project");
        if (path is null) return;

        var project = new Project(path);

        var result = await Mediator.Send(new LaunchProjectRequest(project));

        if (result.IsSuccess)
        {
            Messenger.Send(new ProjectLaunchedMessage(project));
        }
    }
}