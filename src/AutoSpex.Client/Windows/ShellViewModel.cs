﻿using System;
using AutoSpex.Client.Features.Projects;
using AutoSpex.Client.Shared;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Client.Windows;

[UsedImplicitly]
public partial class ShellViewModel : ViewModelBase, IRecipient<ProjectLaunchedMessage>
{
    private readonly IMediator _mediator;

    public ShellViewModel(IMediator mediator)
    {
        _mediator = mediator;

        var path = App.Settings.Get(Setting.OpenProjectPath);
        Project = new Project(new Uri(path));
    }

    [ObservableProperty] private Project _project;

    public void Receive(ProjectLaunchedMessage message)
    {
        if (message.Project == Project) return;
        Project = message.Project;
    }
}