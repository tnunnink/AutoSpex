using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class ProjectPageModel(ProjectObserver project) : PageViewModel, IRecipient<NavigationRequest>
{
    private FileSystemWatcher? _projectWatcher;
    public override string Route => $"{Project.Directory}/{Project.Name}";
    public override bool IsChanged => DetailsPage?.IsChanged is true;

    [ObservableProperty] private ProjectObserver _project = project ?? throw new ArgumentNullException(nameof(project));

    [ObservableProperty] private ObservableCollection<PageViewModel> _menus = [];

    [ObservableProperty] private PageViewModel? _selectedMenu;

    [ObservableProperty] private PageViewModel? _detailsPage;

    [ObservableProperty] private bool _isNavigationOpen = true;
    
    [ObservableProperty] private bool _isStatusDrawerOpen = false;

    public override async Task Load()
    {
        await Navigator.Navigate(() => new SpecsPageModel(Route));
        await Navigator.Navigate(() => new SourcesPageModel(Route));
        await Navigator.Navigate(() => new RunsPageModel(Route));
        await Navigator.Navigate(() => new DetailsPageModel(Route));
    }

    protected override void OnActivated()
    {
        ResetWatcher();
        RegisterWatcher();
        base.OnActivated();
    }

    protected override void OnDeactivated()
    {
        foreach (var menu in Menus.ToList())
            Navigator.Close(menu);

        if (DetailsPage is not null)
            Navigator.Close(DetailsPage);

        ResetWatcher();
        base.OnDeactivated();
    }

    public void Receive(NavigationRequest message)
    {
        switch (message.Action)
        {
            case NavigationAction.Open:
                OpenPage(message);
                break;
            case NavigationAction.Close:
                ClosePage(message);
                break;
            case NavigationAction.Replace:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(message), "Navigation action out of expected range");
        }
    }

    private void OpenPage(NavigationRequest message)
    {
        switch (message.Page)
        {
            case SpecsPageModel or SourcesPageModel or RunsPageModel:
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

    private void ClosePage(NavigationRequest message)
    {
        switch (message.Page)
        {
            case SpecsPageModel or SourcesPageModel or RunsPageModel:
            {
                Menus.Remove(message.Page);
                break;
            }
            case DetailsPageModel:
                DetailsPage = null;
                break;
        }
    }

    private void RegisterWatcher()
    {
        _projectWatcher = Project.Model.CreateWatcher();
        _projectWatcher.Deleted += ProjectDeleted;
        _projectWatcher.Renamed += ProjectRenamed;
        _projectWatcher.Changed += ProjectChanged;
    }

    private void ResetWatcher()
    {
        if (_projectWatcher is null) return;
        _projectWatcher.Deleted -= ProjectDeleted;
        _projectWatcher.Renamed -= ProjectRenamed;
        _projectWatcher.Changed -= ProjectChanged;
    }

    private async void ProjectRenamed(object sender, RenamedEventArgs e)
    {
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            await Prompter.PromptDisconnection(Project.Uri.LocalPath);
            await Navigator.NavigateHome();
        });
    }

    private async void ProjectDeleted(object sender, FileSystemEventArgs e)
    {
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            await Prompter.PromptDisconnection(Project.Uri.LocalPath);
            await Navigator.NavigateHome();
        });
    }

    private void ProjectChanged(object sender, FileSystemEventArgs e)
    {
        //todo this one is tough. Would be cool if we could send message to all pages to refresh or reload,
        //todo but how would we know if it was this user that changed vs another user.
        //Would it hurt to have the changed event always trigger refresh of open pages? Maybe that is how we
        //sync everything as opposed to some other in memory event?
    }
}