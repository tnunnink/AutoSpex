using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Shared;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using DynamicData.Binding;
using HanumanInstitute.MvvmDialogs;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Client.Features.Projects;

[UsedImplicitly]
public partial class ProjectListViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly IMessenger _messenger;
    private readonly IDialogService _dialog;

    private readonly List<Project> _allProjects = new();
    private readonly SourceCache<Project, Uri> _projectCache = new(x => x.Uri);
    private readonly ReadOnlyObservableCollection<Project> _projects;

    public ProjectListViewModel(IMediator mediator, IMessenger messenger, IDialogService dialog)
    {
        _mediator = mediator;
        _messenger = messenger;
        _dialog = dialog;

        _projectCache.Connect()
            .Sort(SortExpressionComparer<Project>.Ascending(t => t.Name))
            .Bind(out _projects)
            .Subscribe();
    }

    public ReadOnlyObservableCollection<Project> Projects => _projects;

    [ObservableProperty] private string _filter = string.Empty;

    [ObservableProperty] private bool _hasRecent;
    
    [RelayCommand]
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

    [RelayCommand(CanExecute = nameof(CanLaunch))]
    private async Task LaunchProject(Project? project)
    {
        if (project is null) return;

        var result = await _mediator.Send(new LaunchProjectRequest(project.Uri));

        if (result.IsSuccess)
        {
            _messenger.Send(new ProjectLaunchedMessage(result.Value));
        }
    }

    private static bool CanLaunch(Project? project) => project is not null && project.File.Exists;
    
    [RelayCommand]
    private async Task LocateProject(Project? project)
    {
        if (project is null) return;

        await _mediator.Send(new LocateProjectRequest(project));
    }
    
    [RelayCommand]
    private async Task CopyPath(Project? project)
    {
        if (project is null) return;

        await _mediator.Send(new CopyPathRequest(project));
        
    }

    [RelayCommand]
    private async Task RemoveProject(Project? project)
    {
        if (project is null) return;

        var result = await _mediator.Send(new RemoveProjectRequest(project));

        if (result.IsSuccess)
        {
            _allProjects.Remove(project);
            _projectCache.Edit(l => l.Remove(project));
        }

        HasRecent = _allProjects.Count > 0;
    }
    
    [RelayCommand]
    private async Task DeleteProject(Project? project)
    {
        if (project is null) return;

        var result = await _mediator.Send(new DeleteProjectRequest(project));

        if (result.IsSuccess)
        {
            _allProjects.Remove(project);
            _projectCache.Edit(l => l.Remove(project));
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