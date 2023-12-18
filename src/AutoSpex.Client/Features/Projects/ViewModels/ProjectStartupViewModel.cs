using AutoSpex.Client.Shared;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HanumanInstitute.MvvmDialogs;
using MediatR;

namespace AutoSpex.Client.Features.Projects;

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

        var result = await _mediator.Send(new CreateProjectRequest(project));

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

        var result = await _mediator.Send(new LaunchProjectRequest(path));

        if (result.IsSuccess)
        {
            _messenger.Send(new ProjectLaunchedMessage(result.Value));
        }
    }
}