using System;
using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Pages;

public abstract partial class NodePageModel : DetailPageModel,
    IRecipient<NodeObserver.Renamed>,
    IRecipient<NodeObserver.Deleted>,
    IRecipient<NavigationRequest>
{
    /// <inheritdoc/>
    protected NodePageModel(NodeObserver node)
    {
        Node = node;
    }

    public override string Route => $"{Node.Type}/{Node.Id}";
    public override string Title => Node.Name;
    public override string Icon => Node.Type.Name;
    public NodeObserver Node { get; }
    public ObservableCollection<PageViewModel> Tabs { get; } = [];

    [ObservableProperty] private PageViewModel? _tab;

    [RelayCommand]
    protected virtual Task Run() => Task.CompletedTask;

    /// <summary>
    /// Close this node if was deleted.
    /// </summary>
    public void Receive(Observer<Node>.Deleted message)
    {
        if (message.Observer.Id != Node.Id) return;
        ForceClose();
    }

    /// <summary>
    /// Notify the <see cref="Title"/> property change when a node is renamed.
    /// </summary>
    public void Receive(NodeObserver.Renamed message)
    {
        if (message.Node.Id != Node.Id) return;
        OnPropertyChanged(nameof(Title));
    }

    public void Receive(NavigationRequest message)
    {
        if (message.Page.Route == Route) return;
        if (!message.Page.Route.StartsWith(Route)) return;

        switch (message.Action)
        {
            case NavigationAction.Open:
                Tabs.Add(message.Page);
                break;
            case NavigationAction.Close:
                Tabs.Remove(message.Page);
                break;
            case NavigationAction.Replace:
                var index = Tabs.IndexOf(message.Page);
                if (index < 0) break;
                Tabs[index] = message.Page;
                break;
            default:
                throw new ArgumentOutOfRangeException($"Navigation action {message.Action} not supported");
        }
    }

    protected override void OnDeactivated()
    {
        base.OnDeactivated();

        foreach (var tab in Tabs.ToList())
            Navigator.Close(tab);
    }

    /// <summary>
    /// Sends a notification to the UI that the node was saved successfully.
    /// </summary>
    protected void NotifySaveSuccess()
    {
        var title = $"{Node.Type} Saved";
        var message = $"{Node.Name} was saved successfully @ {DateTime.Now.ToShortTimeString()}";
        var notification = new Notification(title, message, NotificationType.Success);
        Notifier.Notify(notification);
    }
}