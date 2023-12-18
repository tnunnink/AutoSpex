using System.Threading.Tasks;
using AutoSpex.Client.Features.Projects;
using AutoSpex.Client.Shared;
using Avalonia;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Client.Windows;

[UsedImplicitly]
public class LauncherViewModel : ViewModelBase, IRecipient<ProjectLaunchedMessage>
{
    private readonly IMediator _mediator;

    public LauncherViewModel(IMediator mediator, IMessenger messenger)
    {
        _mediator = mediator;
        messenger.RegisterAll(this);
    }

    public Task<Visual> View => DetermineInitialView();

    private async Task<Visual> DetermineInitialView()
    {
        var result = await _mediator.Send(new GetProjectCountRequest());

        if (result.IsFailed || result.Value == 0)
        {
            return new ProjectStartupView();
        }

        return new ProjectListView();
    }

    public void Receive(ProjectLaunchedMessage message)
    {
        App.Instance.OpenShell();
    }
}