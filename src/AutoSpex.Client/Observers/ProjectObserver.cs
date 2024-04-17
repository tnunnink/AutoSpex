using System;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Observers;

public partial class ProjectObserver(Project project) : Observer<Project>(project)
{
    public Uri Uri => Model.Uri;
    public string Name => Model.Name;
    public string Directory => Model.Directory;
    public bool Exists => Model.Exists;
    public DateTime OpenedOn => Model.OpenedOn;

    [RelayCommand]
    private Task Open() => Manager.OpenProject(this);

    [RelayCommand]
    private Task Locate() => Shell.StorageProvider.ShowInExplorer(Directory);

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
        Messenger.Send(new Deleted(this));
    }
}

