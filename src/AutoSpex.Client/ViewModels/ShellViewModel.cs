using System;
using AutoSpex.Client.Features;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Client.ViewModels;

[UsedImplicitly]
public partial class ShellViewModel : ViewModelBase, IRecipient<ProjectLaunchedMessage>
{
    private readonly IMediator _mediator;

    public ShellViewModel(IMediator mediator)
    {
        _mediator = mediator;

        var path = Settings.App.OpenProject;
        Project = new Project(new Uri(path));
    }

    [ObservableProperty] private Project _project;

    public void Receive(ProjectLaunchedMessage message)
    {
        if (message.Project == Project) return;
        Project = message.Project;
    }
}