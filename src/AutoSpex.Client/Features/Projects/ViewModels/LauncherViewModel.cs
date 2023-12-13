using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using DynamicData.Binding;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Client.Features.Projects;

[UsedImplicitly]
public partial class LauncherViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly IMessenger _messenger;
    private readonly IStoragePicker _picker;
    private readonly IDialogService _dialog;
    
    private readonly List<Project> _allProjects = new();
    private readonly SourceCache<Project, Uri> _projectCache = new(x => x.Uri);
    private readonly ReadOnlyObservableCollection<Project> _projects;

    public LauncherViewModel(IMediator mediator, IMessenger messenger, IStoragePicker picker,
        IDialogService dialog)
    {
        _mediator = mediator;
        _messenger = messenger;
        _picker = picker;
        _dialog = dialog;

        Run = LoadProjects();

        _projectCache.Connect()
            .Sort(SortExpressionComparer<Project>.Ascending(t => t.Name))
            .Bind(out _projects)
            .Subscribe();
    }

    public ReadOnlyObservableCollection<Project> Projects => _projects;

    [ObservableProperty] private string _filter = string.Empty;

    [ObservableProperty] private bool _hasRecent;

    [RelayCommand]
    private async Task NewProject()
    {
        var path = await _dialog.Show<Uri?>(new NewProjectView(), "New Project");
        if (path is null) return;

        var result = await _mediator.Send(new CreateProjectRequest(path));

        if (result.IsSuccess)
        {
            _messenger.Send(new LaunchProjectMessage(result.Value));
        }
    }

    [RelayCommand]
    private async Task OpenProject()
    {
        var file = await _picker.PickProject();
        if (file is null) return;

        var result = await _mediator.Send(new LaunchProjectRequest(file.Path));

        if (result.IsSuccess)
        {
            _messenger.Send(new LaunchProjectMessage(result.Value));
        }
    }

    [RelayCommand(CanExecute = nameof(CanLaunch))]
    private async Task LaunchProject(Project? project)
    {
        if (project is null) return;
        
        var result = await _mediator.Send(new LaunchProjectRequest(project.Uri));

        if (result.IsSuccess)
        {
            _messenger.Send(new LaunchProjectMessage(result.Value));
        }
    }

    private static bool CanLaunch(Project? project) => project is not null && project.File.Exists;

    [RelayCommand(CanExecute = nameof(CanRemove))]
    private async Task RemoveProject(Project? project)
    {
        if (project is null) return;
        
        var result = await _mediator.Send(new RemoveProjectRequest(project.Uri));

        if (result.IsSuccess)
        {
            _allProjects.Remove(project);
            _projectCache.Edit(l => l.Remove(project));
        }
    }
    
    private static bool CanRemove(Project? project) => project is not null;

    private async Task LoadProjects()
    {
        var result = await _mediator.Send(new GetProjectsRequest());

        if (result.IsSuccess)
        {
            _allProjects.Clear();
            _allProjects.AddRange(result.Value);
            _projectCache.AddOrUpdate(_allProjects.ToArray());
        }

        HasRecent = _allProjects.Count > 0;
    }

    partial void OnFilterChanged(string value)
    {
        _projectCache.Edit(innerList =>
        {
            innerList.Clear();

            var filteredProjects = string.IsNullOrEmpty(value)
                ? _allProjects
                : _allProjects.Where(p => p.Name.Contains(value) || p.Uri.AbsolutePath.Contains(value));

            innerList.AddOrUpdate(filteredProjects);
        });
    }
}