using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoSpex.Client.Resources;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentResults;

namespace AutoSpex.Client.Observers;

public partial class RepoObserver : Observer<Repo>,
    IRecipient<RepoObserver.SetConnected>,
    IRecipient<Observer.GetSelected>
{
    private readonly List<SourceObserver> _sources = [];
    private FileSystemWatcher? _watcher;

    public RepoObserver(Repo model) : base(model)
    {
        Sources = new ObserverCollection<Source, SourceObserver>(
            refresh: () => _sources,
            count: () => _sources.Count
        );
        RegisterDisposable(Sources);
    }

    public override Guid Id => Model.RepoId;
    public override string Name => Model.Name;
    public string Location => Model.Location;
    public bool Exists => Model.Exists;
    public ObserverCollection<Source, SourceObserver> Sources { get; }
    public IEnumerable<SourceObserver> Targeted => Sources.Where(s => s.IsChecked);
    public IEnumerable<SourceObserver> Available => Sources.Where(s => !s.IsChecked);
    public int TargetedCount => _sources.Count(s => s.IsChecked);
    public int AvailableCount => _sources.Count(s => !s.IsChecked);

    public ObservableCollection<SourceObserver> SelectedTargets { get; } = [];

    public ObservableCollection<SourceObserver> SelectedAvailable { get; } = [];

    [ObservableProperty] private bool _isConnected;

    [ObservableProperty] private bool _isSyncing;

    [ObservableProperty] private bool _syncRequired;


    /// <inheritdoc />
    public override bool Filter(string? filter)
    {
        return base.Filter(filter) || Model.Location.Satisfies(filter);
    }

    #region Commands

    /// <summary>
    /// Command to connect to this repository location on disc.
    /// This will update the database, sync sources with files on disc, and send message to notify other pages.
    /// </summary>
    [RelayCommand]
    public async Task Connect()
    {
        var result = await Mediator.Send(new ConnectRepo(Model));
        if (Notifier.ShowIfFailed(result)) return;

        //Watch directory for changes to notify UI.
        SetupRepoWatcher();

        //Update connected repo in UI before syncing (syncing called in config page after UI is updated. 
        Messenger.Send(new SetConnected(this));
    }

    /// <summary>
    /// Disconnects the observer by sending a message to set the connection to null and disposes the observer.
    /// </summary>
    [RelayCommand]
    private void Disconnect()
    {
        Messenger.Send(new SetConnected(null));
        Dispose();
    }

    /// <summary>
    /// Command to sync the repo sources with this in memory instance. This will call the find sources and update the
    /// local sources collection for the repo.
    /// </summary>
    [RelayCommand]
    public async Task Sync()
    {
        IsSyncing = true;
        SyncRequired = false;

        if (!Exists)
        {
            Notifier.ShowError("Connection failed", $"'{Location}' not longer exists at ");
            Refresh();
            return;
        }

        await RefreshSources();

        IsSyncing = false;
    }

    /// <summary>
    /// Selects a new location for the repository by prompting the user to choose a directory through a dialog.
    /// If the selected path is empty or null, the method will return without further action.
    /// </summary>
    [RelayCommand]
    private async Task ChangeLocation()
    {
        var location = await Shell.StorageProvider.SelectLocation("Select repository location");
        if (string.IsNullOrEmpty(location)) return;

        var updated = await Mediator.Send(new UpdateRepoLocation(Id, location));
        if (Notifier.ShowIfFailed(updated)) return;

        var repo = new RepoObserver(updated.Value);
        await repo.Connect();
    }

    /// <summary>
    /// Command copy the location of the repository to the clipboard
    /// </summary>
    [RelayCommand]
    private async Task CopyPath()
    {
        if (Shell.Clipboard is null) return;
        await Shell.Clipboard.SetTextAsync(Location);
    }

    /// <summary>
    /// Command to open the file explorer to the location of the repository
    /// </summary>
    [RelayCommand]
    private async Task OpenInExplorer()
    {
        if (!Exists) return;
        await Shell.StorageProvider.ShowInExplorer(Location);
    }

    /// <summary>
    /// Command to open the file explorer to the location of the repository
    /// </summary>
    [RelayCommand]
    private Task Remove()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Command to check all the sources of the repo as targeted/available sources depending on the provided input value.
    /// </summary>
    [RelayCommand]
    private void TargetSources(bool value)
    {
        _sources.ForEach(s => s.IsChecked = value);
        Refresh();
    }

    #endregion

    #region Messages

    /// <summary>
    /// Respond to get selected request for source instances in this page.
    /// </summary>
    public void Receive(GetSelected message)
    {
        if (!Sources.Has(message.Observer)) return;

        foreach (var selected in Sources)
        {
            message.Reply(selected);
        }
    }

    /// <summary>
    /// Receives the SetConnected message and updates the IsConnected property accordingly for the RepoObserver.
    /// </summary>
    /// <param name="message">The SetConnected message to process</param>
    public void Receive(SetConnected message)
    {
        IsConnected = message.Repo is not null && Id == message.Repo.Id;
    }

    /// <summary>
    /// Represents a connected observer for a repository.
    /// </summary>
    public record SetConnected(RepoObserver? Repo);

    #endregion

    /// <summary>
    /// Asynchronously refreshes the list of sources for the current RepoObserver instance.
    /// Clears the existing sources list, fetches new sources for the associated Repo model, creates observer instances for the sources, and updates the UI bindings.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation of refreshing the sources list.</returns>
    private async Task RefreshSources()
    {
        try
        {
            _sources.Clear();
            var sources = await Task.Run(() => Model.FindSources()).ConfigureAwait(false);
            var observers = sources.Select(s => new SourceObserver(s, this)).OrderBy(s => s.Name);
            _sources.AddRange(observers);

            Dispatcher.UIThread.Post(() =>
            {
                //Refresh sources collection and refresh bindings to update the UI
                Sources.Refresh();
                Refresh();
            });
        }
        catch (Exception e)
        {
            Notifier.ShowError("Failed to find sources", e.Message);
        }
    }

    /// <summary>
    /// Sets up a FileSystemWatcher for the repository location and registers event handlers.
    /// The watcher filters specific file types and includes subdirectories.
    /// </summary>
    private void SetupRepoWatcher()
    {
        if (!Model.Exists) return;

        _watcher = new FileSystemWatcher(Model.Location);
        _watcher.Filters.Add("*.L5X");
        _watcher.Filters.Add("*.ACD");
        _watcher.Filters.Add("*.L5Z");
        _watcher.IncludeSubdirectories = true;
        _watcher.EnableRaisingEvents = true;
        _watcher.Changed += OnFileSystemChange;
        _watcher.Created += OnFileSystemChange;
        _watcher.Deleted += OnFileSystemChange;
        _watcher.Renamed += OnFileSystemChange;
        _watcher.Error += OnFileSystemError;
        _watcher = null;
    }

    /// <inheritdoc />
    protected override void OnDeactivated()
    {
        base.OnDeactivated();

        if (_watcher is null) return;
        _watcher.Changed -= OnFileSystemChange;
        _watcher.Created -= OnFileSystemChange;
        _watcher.Deleted -= OnFileSystemChange;
        _watcher.Renamed -= OnFileSystemChange;
        _watcher.Error -= OnFileSystemError;
        _watcher.Dispose();
        _watcher = null;
    }

    /// <summary>
    /// When we detect a file system update we need refresh local state.
    /// </summary>
    private void OnFileSystemChange(object sender, FileSystemEventArgs e)
    {
        Dispatcher.UIThread.Invoke(() => { SyncRequired = Model.Exists; });
    }

    /// <summary>
    /// Event handler for file system errors occurred in the FileSystemWatcher.
    /// Displays an error notification and triggers a refresh.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="ErrorEventArgs"/> instance containing the error event data.</param>
    private void OnFileSystemError(object sender, ErrorEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            Notifier.ShowError("Connection failed", $"'{Name}' encountered error {e.GetException().Message}");
            Refresh();
        });
    }

    /// <inheritdoc />
    protected override Task<Result> DeleteItems(IEnumerable<Observer> observers)
    {
        return Mediator.Send(new RemoveRepos(observers.Cast<RepoObserver>().Select(n => n.Model)));
    }

    protected override IEnumerable<MenuActionItem> GenerateMenuItems()
    {
        yield return new MenuActionItem
        {
            Header = "Sync",
            Icon = Resource.Find("IconLineRotate"),
            Command = SyncCommand,
            DetermineVisibility = () => Exists && IsConnected
        };

        yield return new MenuActionItem
        {
            Header = "Disconnect",
            Icon = Resource.Find("IconLineBan"),
            Command = DisconnectCommand,
            DetermineVisibility = () => IsConnected
        };

        yield return new MenuActionItem
        {
            Header = "Change location",
            Icon = Resource.Find("IconFilledFolderOpen"),
            Command = ChangeLocationCommand
        };

        yield return new MenuActionItem
        {
            Header = "Copy path",
            Icon = Resource.Find("IconFilledCopy"),
            Command = CopyPathCommand,
            DetermineVisibility = () => Exists
        };

        yield return new MenuActionItem
        {
            Header = "Open in explorer",
            Icon = Resource.Find("IconLineLaunch"),
            Command = OpenInExplorerCommand,
            DetermineVisibility = () => Exists
        };

        yield return new MenuActionItem
        {
            Header = "Remove",
            Icon = Resource.Find("IconFilledTrash"),
            Command = DeleteCommand,
            Classes = "danger"
        };
    }

    public static implicit operator RepoObserver(Repo model) => new(model);
    public static implicit operator Repo(RepoObserver observer) => observer.Model;
}