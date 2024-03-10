using System;
using System.Collections.ObjectModel;
using System.IO;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages.Projects;

[UsedImplicitly]
public partial class ProjectPageModel(ProjectObserver project) : PageViewModel, IRecipient<NavigationRequest>
{
    private FileSystemWatcher? _projectWatcher;

    [ObservableProperty] private ProjectObserver _project = project ?? throw new ArgumentNullException(nameof(project));

    [ObservableProperty] private ObservableCollection<PageViewModel> _menus = [];

    [ObservableProperty] private PageViewModel? _selectedMenu;

    [ObservableProperty] private PageViewModel? _detailsPage;


    public override string Route => $"{Project.Directory}/{Project.Name}";

    public override bool IsChanged => DetailsPage?.IsChanged ?? false;

    public override void AcceptChanges()
    {
        DetailsPage?.AcceptChanges();
    }

    public override async Task Load()
    {
        await Navigator.Navigate(() => new SpecsPageModel());
        await Navigator.Navigate(() => new SourcesPageModel());
        await Navigator.Navigate(() => new RunnerListPageModel());
        await Navigator.Navigate(() => new DetailsPageModel());
    }

    protected override void OnActivated()
    {
        base.OnActivated();
        ResetWatcher();
        RegisterWatcher();
    }

    protected override void OnDeactivated()
    {
        base.OnDeactivated();
        ResetWatcher();
    }

    public void Receive(NavigationRequest message)
    {
        switch (message.Page)
        {
            case SpecsPageModel or SourcesPageModel or RunnerListPageModel:
            {
                if (!Menus.Contains(message.Page))
                    Menus.Add(message.Page);
                SelectedMenu ??= message.Page;
                break;
            }
            case DetailsPageModel:
                DetailsPage = message.Page;
                break;
        }
    }

    private void ResetWatcher()
    {
        if (_projectWatcher is not null)
        {
            _projectWatcher.Deleted -= ProjectDeleted;
        }
    }

    private void RegisterWatcher()
    {
        _projectWatcher = Project.Model.CreateWatcher();
        _projectWatcher.Deleted += ProjectDeleted;
        _projectWatcher.Renamed += ProjectRenamed;
        _projectWatcher.Changed += ProjectChanged;
    }

    private void ProjectChanged(object sender, FileSystemEventArgs e)
    {
        //todo this one is tough. Would be cool if we could send message to all pages to refresh or reload, but how would we know if it was this user that changed vs another user.
    }

    private void ProjectRenamed(object sender, RenamedEventArgs e)
    {
        //todo we might need to notify but we definitely need to reset the "open" project to get the correct data source path.
    }

    private async void ProjectDeleted(object sender, FileSystemEventArgs e)
    {
        //todo we need notify the user and basically close out this project and return the home.
        await Navigator.NavigateHome();
        IsActive = false;
        Navigator.Close(this);
    }
}