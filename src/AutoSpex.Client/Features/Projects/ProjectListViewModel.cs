using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using DynamicData.Binding;
using HanumanInstitute.MvvmDialogs;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Client.Features;

[UsedImplicitly]
public partial class ProjectListViewModel : ViewModelBase
{
    private readonly IDialogService _dialog;

    private readonly List<Project> _allProjects = [];
    private readonly SourceCache<Project, Uri> _projectCache = new(x => x.Uri);
    private readonly ReadOnlyObservableCollection<Project> _projects;

    public ProjectListViewModel(IMediator mediator, IMessenger messenger, IDialogService dialog)
    {
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
        var result = await Mediator.Send(new ListProjects());

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

        var result = await Mediator.Send(new CreateProject(project));

        if (result.IsSuccess)
        {
            Messenger.Send(new ProjectLaunchedMessage(project));
        }
    }

    [RelayCommand]
    private async Task OpenProject()
    {
        var path = await _dialog.ShowSelectProjectDialog("Select Spex Project");
        if (path is null) return;

        var project = new Project(path);

        var result = await Mediator.Send(new LaunchProjectRequest(project));

        if (result.IsSuccess)
        {
            Messenger.Send(new ProjectLaunchedMessage(project));
        }
    }

    [RelayCommand(CanExecute = nameof(CanLaunch))]
    private async Task LaunchProject(Project? project)
    {
        if (project is null) return;

        var result = await Mediator.Send(new LaunchProjectRequest(project));

        if (result.IsSuccess)
        {
            Messenger.Send(new ProjectLaunchedMessage(project));
        }
    }

    private static bool CanLaunch(Project? project) => project is not null && project.Exists;

    [RelayCommand]
    private async Task LocateProject(Project? project)
    {
        if (project is null) return;

        await Mediator.Send(new ProjectRequest.LocateProject(project));
    }

    [RelayCommand]
    private async Task CopyPath(Project? project)
    {
        if (project is null) return;

        await Mediator.Send(new ProjectRequest.CopyPath(project));
    }

    [RelayCommand]
    private async Task RemoveProject(Project? project)
    {
        if (project is null) return;

        var result = await Mediator.Send(new RemoveProject(project));

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

        var result = await Mediator.Send(new DeleteProject(project));

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