using System;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
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
    private void Open() => Messenger.Send(new OpenMessage(this));

    [RelayCommand]
    private void Locate() => Messenger.Send(new LocateMessage(this));

    [RelayCommand]
    private void CopyPath() => Messenger.Send(new CopyPathMessage(this));
    
    //todo pin/unpin

    [RelayCommand]
    private void Remove() => Messenger.Send(new RemoveMessage(this));
    
    public record OpenMessage(ProjectObserver Project);
    public record RemoveMessage(ProjectObserver Project);
    public record LocateMessage(ProjectObserver Project);
    public record CopyPathMessage(ProjectObserver Project);
    public record PinMessage(ProjectObserver Project);
    public record UnpinMessage(ProjectObserver Project);
}

