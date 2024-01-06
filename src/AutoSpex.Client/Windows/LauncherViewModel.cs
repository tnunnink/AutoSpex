using System.Threading.Tasks;
using AutoSpex.Client.Messages;
using AutoSpex.Client.Shared;
using AutoSpex.Persistence;
using Avalonia;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;
using MediatR;
using ProjectListView = AutoSpex.Client.Views.ProjectListView;
using ProjectStartupView = AutoSpex.Client.Views.ProjectStartupView;

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
        var result = await _mediator.Send(new GetProjectCount());

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