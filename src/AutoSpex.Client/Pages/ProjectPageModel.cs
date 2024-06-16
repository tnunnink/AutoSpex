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
public partial class ProjectPageModel(ProjectObserver project) : PageViewModel,
    IRecipient<NavigationRequest>,
    IRecipient<RunObserver.OpenRun>
{
    private RunnerPageModel? _runner;
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

    /// <inheritdoc />
    public override async Task Load()
    {
        await Navigator.Navigate(() => new NavigationPageModel(Route, NodeType.Spec));
        await Navigator.Navigate(() => new NavigationPageModel(Route, NodeType.Source));
        await Navigator.Navigate(() => new NavigationPageModel(Route, NodeType.Run));
        await Navigator.Navigate(() => new DetailsPageModel(Route));
        await Navigator.Navigate<RunnerPageModel>();
    }

    /// <inheritdoc />
    protected override void OnActivated()
    {
        ResetWatcher();
        RegisterWatcher();
        base.OnActivated();
    }

    /// <inheritdoc />
    protected override void OnDeactivated()
    {
        foreach (var menu in Menus.ToList())
            Navigator.Close(menu);

        if (DetailsPage is not null)
            Navigator.Close(DetailsPage);

        if (_runner is not null)
            Navigator.Close(_runner);

        ResetWatcher();
        base.OnDeactivated();
    }

    /// <summary>
    /// Toggles the navigation drawer view of the main project page.
    /// </summary>
    [RelayCommand]
    private void ToggleNavigationDrawer()
    {
        IsNavigationOpen = !IsNavigationOpen;
    }

    /// <summary>
    /// Navigates the runner footer page into view (if not currently shown) and either opens or toggles the status bar
    /// drawer view.
    /// </summary>
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

    /// <summary>
    /// Handles the navigation requests for this main project page.
    /// </summary>
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

    /// <summary>
    /// When the open run message is sent, ensure the runner footer page is opened and then reply with a flag that the
    /// message as received.
    /// </summary>
    /// <param name="message">The <see cref="RunObserver.OpenRun"/> messages sent to trigger opening and loading
    /// of the provided run.</param>
    public async void Receive(RunObserver.OpenRun message)
    {
        await Navigator.Navigate(() => new RunnerPageModel());
        message.Reply(true);
    }

    /// <summary>
    /// Opens the requested page depending on the model that is passed in.
    /// </summary>
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
            case RunnerPageModel runner:
                ShowRunner(runner);
                break;
        }
    }

    /// <summary>
    /// Closes the requested page depending on the model that is passed in.
    /// </summary>
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
                HideRunner();
                break;
        }
    }

    /// <summary>
    /// Shows the runner page in the status bar drawer. If this is the first time the runner is called (on startup) then
    /// just return without opening the drawer.
    /// </summary>
    private void ShowRunner(RunnerPageModel runner)
    {
        FooterPage = runner;

        if (_runner is null)
        {
            _runner = runner;
            return;
        }

        IsStatusDrawerOpen = true;
    }

    /// <summary>
    /// Hides the runner page in the status bar drawer if that is indeed the current footer page being shown.
    /// </summary>
    private void HideRunner()
    {
        if (FooterPage?.Route != nameof(RunnerPageModel)) return;
        FooterPage = null;
        IsStatusDrawerOpen = false;
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