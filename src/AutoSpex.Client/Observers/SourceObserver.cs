using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AutoSpex.Client.Resources;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentResults;
using L5Sharp.Core;

namespace AutoSpex.Client.Observers;

public partial class SourceObserver : Observer<Source>
{
    private readonly FileSystemWatcher? _watcher;
    private readonly FileInfo _info;

    /// <inheritdoc/>
    public SourceObserver(Source source) : base(source)
    {
        Overrides = new ObserverCollection<Variable, VariableObserver>(Model.Overrides, v => new VariableObserver(v));
        Track(nameof(Name));
        Track(Overrides);

        _watcher = source.CreateWatcher();
        _info = new FileInfo(Model.Uri.LocalPath);

        if (_watcher is null) return;
        _watcher.Renamed += OnRenamed;
        _watcher.Deleted += OnDeleted;
        _watcher.Created += OnCreated;
    }

    public override Guid Id => Model.SourceId;
    public override string Icon => nameof(Source);

    public override string Name
    {
        get => Model.Name;
        set => SetProperty(Model.Name, value, Model, (s, v) => s.Name = v);
    }

    public string LocalPath => Model.Uri.LocalPath;
    public bool Exists => Model.Exists;
    public string Size => _info.Exists ? $"{_info.Length / 1024:N0} KB" : string.Empty;
    public string CreatedOn => _info.Exists ? File.GetCreationTime(Model.Uri.LocalPath).ToString("g") : string.Empty;
    public ObserverCollection<Variable, VariableObserver> Overrides { get; }

    public override bool Filter(string? filter)
    {
        FilterText = filter;
        return Name.Satisfies(filter) || LocalPath.Satisfies(filter);
    }

    public Task<Result<L5X>> LoadContent()
    {
        return Task.Run(() =>
        {
            try
            {
                if (!Exists) return Result.Fail("Source does not exist");
                var content = Model.Load();
                return Result.Ok(content);
            }
            catch (Exception e)
            {
                return Result.Fail(e.Message);
            }
        });
    }

    [RelayCommand]
    private async Task Locate()
    {
        if (string.IsNullOrEmpty(Model.Directory)) return;
        await Shell.StorageProvider.ShowInExplorer(Model.Directory);
    }

    [RelayCommand]
    private async Task CopyPath()
    {
        if (string.IsNullOrEmpty(Model.Directory)) return;
        if (Shell.Clipboard is null) return;
        await Shell.Clipboard.SetTextAsync(Model.Directory);
    }

    protected override Task Duplicate()
    {
        var duplicate = new SourceObserver(new Source(Model.Uri));
        Messenger.Send(new Duplicated(this, duplicate));
        return Task.CompletedTask;
    }

    protected override Task Delete()
    {
        IsActive = false;
        Messenger.Send(new Deleted(this));
        return Task.CompletedTask;
    }

    protected override void OnDeactivated()
    {
        base.OnDeactivated();

        if (_watcher is null) return;
        _watcher.Renamed -= OnRenamed;
        _watcher.Deleted -= OnDeleted;
        _watcher.Created -= OnDeleted;

        Model.Release();
    }

    protected override IEnumerable<MenuActionItem> GenerateMenuItems()
    {
        yield return new MenuActionItem
        {
            Header = "Inspect",
            Icon = Resource.Find("IconFilledBinoculars"),
            DetermineVisibility = () => Exists
        };

        yield return new MenuActionItem
        {
            Header = "Open In Explorer",
            Icon = Resource.Find("IconFilledFolder"),
            Command = LocateCommand,
            DetermineVisibility = () => Exists
        };

        yield return new MenuActionItem
        {
            Header = "Rename",
            Icon = Resource.Find("IconLineInputText"),
            Command = RenameCommand,
            Gesture = new KeyGesture(Key.E, KeyModifiers.Control),
        };

        yield return new MenuActionItem
        {
            Header = "Duplicate",
            Icon = Resource.Find("IconFilledClone"),
            Command = DuplicateCommand,
            Gesture = new KeyGesture(Key.D, KeyModifiers.Control),
            DetermineVisibility = () => Exists
        };

        yield return new MenuActionItem
        {
            Header = "Delete",
            Icon = Resource.Find("IconFilledTrash"),
            Classes = "danger",
            Command = DeleteCommand,
            Gesture = new KeyGesture(Key.Delete)
        };
    }

    protected override IEnumerable<MenuActionItem> GenerateContextItems()
    {
        yield return new MenuActionItem
        {
            Header = "Inspect",
            Icon = Resource.Find("IconFilledBinoculars"),
            DetermineVisibility = () => IsSolelySelected && Exists
        };

        yield return new MenuActionItem
        {
            Header = "Open In Explorer",
            Icon = Resource.Find("IconFilledFolderOpen"),
            Command = LocateCommand,
            DetermineVisibility = () => IsSolelySelected && Exists
        };

        yield return new MenuActionItem
        {
            Header = "Rename",
            Icon = Resource.Find("IconFilledPen"),
            Command = RenameCommand,
            Gesture = new KeyGesture(Key.E, KeyModifiers.Control),
            DetermineVisibility = () => IsSolelySelected
        };

        yield return new MenuActionItem
        {
            Header = "Duplicate",
            Icon = Resource.Find("IconFilledClone"),
            Command = DuplicateCommand,
            Gesture = new KeyGesture(Key.D, KeyModifiers.Control),
            DetermineVisibility = () => IsSolelySelected && Exists
        };

        yield return new MenuActionItem
        {
            Header = "Delete",
            Icon = Resource.Find("IconFilledTrash"),
            Classes = "danger",
            Command = DeleteCommand,
            Gesture = new KeyGesture(Key.Delete)
        };
    }

    private void OnRenamed(object sender, RenamedEventArgs e) => OnPropertyChanged(nameof(Exists));

    private void OnDeleted(object sender, FileSystemEventArgs e) => OnPropertyChanged(nameof(Exists));

    private void OnCreated(object sender, FileSystemEventArgs e) => OnPropertyChanged(nameof(Exists));
}