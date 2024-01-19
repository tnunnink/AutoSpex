using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using AutoSpex.Client.Messages;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using DynamicData.Binding;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class HomePageModel : PageViewModel, 
    IRecipient<ProjectOpenMessage>,
    IRecipient<ProjectRemoveMessage>,
    IRecipient<ProjectLocateMessage>,
    IRecipient<ProjectCopyPathMessage>
{
    private readonly Launcher _launcher;
    private readonly List<ProjectObserver> _allProjects = [];
    private readonly SourceCache<ProjectObserver, Uri> _projectCache = new(x => x.Uri);
    private readonly ReadOnlyObservableCollection<ProjectObserver> _projects;

    public HomePageModel(Launcher launcher)
    {
        _launcher = launcher ?? throw new ArgumentNullException(nameof(launcher));
        _projectCache.Connect()
            .Sort(SortExpressionComparer<ProjectObserver>.Ascending(t => t.OpenedOn))
            .Bind(out _projects)
            .Subscribe();
    }
    
    public ReadOnlyObservableCollection<ProjectObserver> Projects => _projects;
    
    [ObservableProperty] private Project? _project;

    [ObservableProperty] private string _filter = string.Empty;

    [ObservableProperty] private bool _hasRecent;
    
    public async void Receive(ProjectOpenMessage message)
    {
        await LaunchProject(message.Project);
    }
    
    public async void Receive(ProjectLocateMessage message)
    {
        await LocateProject(message.Project);
    }
    
    public async void Receive(ProjectCopyPathMessage message)
    {
        await CopyPath(message.Project);
    }
    
    public async void Receive(ProjectRemoveMessage message)
    {
        await RemoveProject(message.Project);
    }

    protected override async Task Load()
    {
        var result = await Mediator.Send(new ListProjects());
        if (result.IsFailed) return;

        var observers = result.Value.Select(v => new ProjectObserver(v));
        
        _allProjects.Clear();
        _allProjects.AddRange(observers);
        _projectCache.AddOrUpdate(_allProjects.ToArray());

        HasRecent = _allProjects.Count > 0;
    }

    [RelayCommand]
    private void NewProject()
    {
        Navigator.Navigate<NewProjectPageModel>();
    }

    [RelayCommand]
    private async Task OpenProject()
    {
        var path = await Shell.StorageProvider.SelectProjectUri();
        if (path is null) return;

        var project = new Project(path);

        await LaunchProject(new ProjectObserver(project));
    }
    
    private async Task LaunchProject(ProjectObserver? project)
    {
        if (project is null || !project.Exists) return;
        
        var result = await _launcher.Launch(project.Model, CancellationToken.None); //todo how are se sending tokens in.

        if (result.IsFailed) return;
        await Navigator.Navigate(() => new ProjectPageModel(project));
    }
    
    private async Task RemoveProject(ProjectObserver? project)
    {
        if (project is null) return;
        
        var result = await Mediator.Send(new RemoveProject(project.Model));

        if (result.IsSuccess)
        {
            _allProjects.Remove(project);
            _projectCache.Edit(l => l.Remove(project));
        }

        HasRecent = _allProjects.Count > 0;
    }
    
    private async Task LocateProject(ProjectObserver? project)
    {
        if (project is null) return;
        await Shell.StorageProvider.ShowInExplorer(project.Directory);
    }
    
    private async Task CopyPath(ProjectObserver? project)
    {
        if (project is null || Shell.Clipboard is null) return;
        await Shell.Clipboard.SetTextAsync(project.Directory);
    }
    
    partial void OnFilterChanged(string value)
    {
        _projectCache.Edit(innerList =>
        {
            innerList.Clear();

            var filtered = string.IsNullOrEmpty(value)
                ? _allProjects
                : _allProjects.Where(p => p.Name.Contains(value) || p.Directory.Contains(value));

            innerList.AddOrUpdate(filtered);
        });
    }
}