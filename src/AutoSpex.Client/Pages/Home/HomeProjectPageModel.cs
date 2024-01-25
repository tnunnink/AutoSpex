using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Pages.Projects;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages.Home;

[UsedImplicitly]
public partial class HomeProjectPageModel(Launcher launcher) : PageViewModel,
    IRecipient<ProjectObserver.OpenMessage>,
    IRecipient<ProjectObserver.RemoveMessage>,
    IRecipient<ProjectObserver.LocateMessage>,
    IRecipient<ProjectObserver.CopyPathMessage>
{
    private readonly Launcher _launcher = launcher ?? throw new ArgumentNullException(nameof(launcher));

    [ObservableProperty] private ObservableCollection<ProjectObserver> _projects = [];

    [ObservableProperty] private ProjectObserver? _project;

    public async void Receive(ProjectObserver.OpenMessage message)
    {
        await LaunchProject(message.Project);
    }

    public async void Receive(ProjectObserver.LocateMessage message)
    {
        await LocateProject(message.Project);
    }

    public async void Receive(ProjectObserver.CopyPathMessage message)
    {
        await CopyPath(message.Project);
    }

    public async void Receive(ProjectObserver.RemoveMessage message)
    {
        await RemoveProject(message.Project);
    }

    public override async Task Load()
    {
        var result = await Mediator.Send(new ListProjects());
        if (result.IsFailed) return;
        Projects = new ObservableCollection<ProjectObserver>(result.Value.Select(v => new ProjectObserver(v)));
    }

    [RelayCommand]
    private void CreateProject()
    {
        Navigator.Navigate<CreateProjectPageModel>();
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
        if (result.IsFailed) return;
        Projects.Remove(project);
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
}