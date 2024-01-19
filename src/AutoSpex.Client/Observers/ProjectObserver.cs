using System;
using AutoSpex.Client.Messages;
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
    private void Open() => Messenger.Send(new ProjectOpenMessage(this));

    [RelayCommand]
    private void Locate() => Messenger.Send(new ProjectLocateMessage(this));

    [RelayCommand]
    private void CopyPath() => Messenger.Send(new ProjectCopyPathMessage(this));
    
    //todo pin/unpin

    [RelayCommand]
    private void Remove() => Messenger.Send(new ProjectRemoveMessage(this));
}