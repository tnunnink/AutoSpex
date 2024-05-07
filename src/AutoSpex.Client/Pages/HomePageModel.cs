using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class HomePageModel : PageViewModel,
    IRecipient<ProjectObserver.Created>,
    IRecipient<ProjectObserver.Deleted>
{
    public override string Route => "Home";
    public ObserverCollection<Project, ProjectObserver> Projects { get; } = [];

    public override async Task Load()
    {
        var result = await Mediator.Send(new ListProjects());
        if (result.IsFailed) return;
        Projects.AddRange(result.Value.Select(v => new ProjectObserver(v)));
    }

    [RelayCommand]
    private async Task CreateProject()
    {
        var project = await Prompter.Show<ProjectObserver?>(() => new CreateProjectPageModel());
        if (project is null) return;
        await project.Connect();
    }

    [RelayCommand]
    private async Task OpenProject()
    {
        var path = await Shell.StorageProvider.SelectProjectUri();
        if (path is null) return;
        
        var project = new ProjectObserver(new Project(path));
        await project.Connect();
        
        Projects.Add(project);
    }
    
    public void Receive(Observer<Project>.Created message)
    {
        if (message.Observer is not ProjectObserver project) return;
        Projects.Add(project);
    }

    public void Receive(Observer<Project>.Deleted message)
    {
        if (message.Observer is not ProjectObserver project) return;
        Projects.Remove(project);
    }
}