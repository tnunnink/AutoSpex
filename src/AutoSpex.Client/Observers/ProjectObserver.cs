using System;
using System.IO;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Observers;

public partial class ProjectObserver : Observer<Project>
{
    private readonly FileSystemWatcher? _projectWatcher;

    /// <inheritdoc/>
    public ProjectObserver(Project project) : base(project)
    {
        _projectWatcher = project.CreateWatcher();
        if (_projectWatcher is null) return;
        _projectWatcher.Renamed += OnProjectRenamed;
        _projectWatcher.Deleted += OnProjectDeleted;
        _projectWatcher.Created += OnProjectCreated;
    }

    public Uri Uri => Model.Path;
    public string Name => Model.Name;
    public string Directory => Model.Directory;
    public bool Exists => Model.Exists;
    public DateTime CreatedOn => File.GetCreationTime(Uri.LocalPath);
    public DateTime UpdatedOn => File.GetLastWriteTime(Uri.LocalPath);

    public bool Pinned
    {
        get => Model.Pinned;
        set => SetProperty(Model.Pinned, value, Model, (p, v) => p.Pinned = v);
    }

    [RelayCommand(CanExecute = nameof(CanConnect))]
    public async Task Connect()
    {
        if (!Exists) return;

        var action = await Mediator.Send(new EvaluateProject(Model));
        if (action.IsFailed)
        {
            //todo prompt user here and return
            /*return Result.Fail(
                                $"Failed to evaluate the current version of the project '{project.Name}'. " +
                                $"Make sure this is a valid Spex project file and that the file exists locally.")
                            .WithErrors(action.Errors);*/
        }

        if (action.Value == ProjectAction.MigrationRequired)
        {
            await HandleMigrationRequired(Model);
        }

        if (action.Value == ProjectAction.UpdateRequired)
        {
            //todo handle
        }

        if (action.Value == ProjectAction.UpdateSuggested)
        {
            //todo handle
        }

        var result = await Mediator.Send(new OpenProject(Model));
        if (result.IsFailed) return;
        await Navigator.Navigate(this);
    }

    public bool CanConnect() => Exists;

    [RelayCommand]
    private Task Locate() => Shell.StorageProvider.ShowInExplorer(Directory);

    [RelayCommand]
    private async Task Pin()
    {
        Pinned = true;
        await Mediator.Send(new UpdateProjectPin(Model));
    }

    [RelayCommand]
    private async Task Unpin()
    {
        Pinned = false;
        await Mediator.Send(new UpdateProjectPin(Model));
    }

    [RelayCommand]
    private async Task CopyPath()
    {
        if (Shell.Clipboard is null) return;
        await Shell.Clipboard.SetTextAsync(Directory);
    }

    protected override async Task Delete()
    {
        var result = await Mediator.Send(new RemoveProject(Model));
        if (result.IsFailed) return;
        IsActive = false;
        Messenger.Send(new Deleted(this));
    }

    protected override void OnDeactivated()
    {
        base.OnDeactivated();

        if (_projectWatcher is null) return;
        _projectWatcher.Renamed -= OnProjectRenamed;
        _projectWatcher.Deleted -= OnProjectDeleted;
        _projectWatcher.Created -= OnProjectDeleted;
    }

    private async Task HandleMigrationRequired(Project project)
    {
        var migrate = await Prompter.PromptMigrate(project.Name);
        if (migrate is not true) return;
        await Mediator.Send(new MigrateProject(project));
    }

    private void OnProjectRenamed(object sender, RenamedEventArgs e) => OnPropertyChanged(nameof(Exists));

    private void OnProjectDeleted(object sender, FileSystemEventArgs e) => OnPropertyChanged(nameof(Exists));

    private void OnProjectCreated(object sender, FileSystemEventArgs e) => OnPropertyChanged(nameof(Exists));

    /// <summary>
    /// A message that is sent to indicate that the file project has changed. When this happens we want certain views
    /// to refresh their state. This keeps the UI in sync in case and makes for concurrent users to be able to work
    /// on the same project without some fighting for state changes.
    /// </summary>
    public record Changed;
}