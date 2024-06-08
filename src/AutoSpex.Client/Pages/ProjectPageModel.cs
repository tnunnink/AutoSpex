using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

    [ObservableProperty] private PageViewModel? _footerPage;

    [ObservableProperty] private bool _isNavigationOpen = true;

    [ObservableProperty] private bool _isStatusDrawerOpen;

    public override async Task Load()
    {
        await Navigator.Navigate(() => new NavigationPageModel(Route, NodeType.Spec));
        await Navigator.Navigate(() => new NavigationPageModel(Route, NodeType.Source));
        await Navigator.Navigate(() => new NavigationPageModel(Route, NodeType.Run));
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

    [RelayCommand]
    private async Task NavigateRunner()
    {
        if (FooterPage is null || FooterPage.Route != nameof(RunnerPageModel))
        {
            await Navigator.Navigate(() => new RunnerPageModel());
            return;
        }

        IsStatusDrawerOpen = !IsStatusDrawerOpen;
    }

    [RelayCommand]
    private void ToggleNavigationDrawer()
    {
        IsNavigationOpen = !IsNavigationOpen;
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
            case NavigationPageModel:
                if (!Menus.Contains(message.Page))
                    Menus.Add(message.Page);
                SelectedMenu ??= message.Page;
                break;
            case DetailsPageModel:
                DetailsPage = message.Page;
                break;
            case RunnerPageModel:
                FooterPage = message.Page;
                IsStatusDrawerOpen = true;
                break;
        }
    }

    private void ClosePage(NavigationRequest message)
    {
        switch (message.Page)
        {
            case NavigationPageModel:
                Menus.Remove(message.Page);
                break;
            case DetailsPageModel:
                DetailsPage = null;
                break;
            case RunnerPageModel:
                if (FooterPage?.Route != nameof(RunnerPageModel)) return;
                FooterPage = null;
                IsStatusDrawerOpen = false;
                break;
        }
    }

    private void RegisterWatcher()
    {
        _projectWatcher = Project.Model.CreateWatcher();
        if (_projectWatcher is null) return;
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
        Messenger.Send(new ProjectObserver.Changed());
    }
}