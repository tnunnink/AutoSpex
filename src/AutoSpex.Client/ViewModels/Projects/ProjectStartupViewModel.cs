using AutoSpex.Client.Messages;
using AutoSpex.Client.Requests;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HanumanInstitute.MvvmDialogs;
using MediatR;

namespace AutoSpex.Client.ViewModels;

public partial class ProjectStartupViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly IMessenger _messenger;
    private readonly IDialogService _dialog;

    public ProjectStartupViewModel(IMediator mediator, IMessenger messenger, IDialogService dialog)
    {
        _mediator = mediator;
        _messenger = messenger;
        _dialog = dialog;
    }

    [RelayCommand]
    private async Task NewProject()
    {
        var path = await _dialog.ShowNewProjectDialog();
        if (path is null) return;

        var project = new Project(path);

        var result = await _mediator.Send(new CreateProject(project));

        if (result.IsSuccess)
        {
            _messenger.Send(new ProjectLaunchedMessage(project));
        }
        
        //todo it should notify if failed, but should we do something else here? Or just let them try again?
    }

    [RelayCommand]
    private async Task OpenProject()
    {
        var path = await _dialog.ShowSelectProjectDialog("Select Spex Project");
        if (path is null) return;

        var project = new Project(path);

        var result = await _mediator.Send(new LaunchProjectRequest(project));

        if (result.IsSuccess)
        {
            _messenger.Send(new ProjectLaunchedMessage(project));
        }
    }
}