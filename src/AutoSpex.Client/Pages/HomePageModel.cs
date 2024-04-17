using System;
using System.Collections.ObjectModel;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class HomePageModel : PageViewModel
{
    public override string Route => nameof(HomePageModel);
    public ObservableCollection<ProjectObserver> Projects { get; } = [];
    
    public override async Task Load()
    {
        var projects = await Manager.GetProjects();

        foreach (var project in projects)
        {
            Projects.Add(project);
        }
    }

    [RelayCommand]
    private async Task CreateProject()
    {
        var project = await Prompter.Show<ProjectObserver?>(() => new CreateProjectPageModel());
        if (project is null) return;
        await Manager.OpenProject(project);
    }

    [RelayCommand]
    private async Task OpenProject()
    {
        var path = await Shell.StorageProvider.SelectProjectUri();
        if (path is null) return;
        var project = new ProjectObserver(new Project(path));
        await Manager.OpenProject(project);
    }
}