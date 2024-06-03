using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentResults;

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
    public override bool IsChanged => base.IsChanged || Tabs.Any(t => t.IsChanged);
    public override bool IsErrored => base.IsErrored || Tabs.Any(t => t.IsErrored);
    public NodeObserver Node { get; }
    public ObservableCollection<DetailPageModel> Tabs { get; } = [];

    [ObservableProperty] private DetailPageModel? _tab;

    /// <summary>
    /// When a node page is loaded, it will forward the call to its child tabs to be loaded.
    /// </summary>
    public override async Task Load()
    {
        await NavigateTabs();
        SaveCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    protected virtual Task Run() => Task.CompletedTask;

    /// <inheritdoc />
    public override async Task<Result> Save()
    {
        var result = Result.Ok();

        foreach (var tab in Tabs)
        {
            var saved = await tab.Save();
            result = Result.Merge(result, saved);
        }

        if (result.IsFailed)
        {
            NotifySaveFailed(result);
        }
        else
        {
            NotifySaveSuccess();
            AcceptChanges();
        }

        return result;
    }

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

    /// <summary>
    /// Handles the navigation request for this node detail page. Only handle tabs that start with the same route,
    /// meaning it is a child tab of this page.
    /// </summary>
    /// <param name="message"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void Receive(NavigationRequest message)
    {
        if (message.Page is not DetailPageModel page) return;
        if (message.Page.Route == Route) return;
        if (!message.Page.Route.StartsWith(Route)) return;

        switch (message.Action)
        {
            case NavigationAction.Open:
                Tabs.Add(page);
                Track(page);
                break;
            case NavigationAction.Close:
                Tabs.Remove(page);
                Forget(page);
                break;
            /*case NavigationAction.Replace:
                var index = Tabs.IndexOf(message.Page);
                if (index < 0) break;
                Tabs[index] = message.Page;
                break;*/
            default:
                throw new ArgumentOutOfRangeException($"Navigation action {message.Action} not supported");
        }
    }

    /// <summary>
    /// When this node page is closed and deactivated we also want to close any child tab pages.
    /// </summary>
    protected override void OnDeactivated()
    {
        base.OnDeactivated();

        foreach (var tab in Tabs.ToList())
            Navigator.Close(tab);
    }

    /// <summary>
    /// Handles requesting navigation of any child tabs for this node page.
    /// </summary>
    protected virtual Task NavigateTabs() => Task.CompletedTask;

    /// <summary>
    /// Sends a notification to the UI that the node was saved successfully.
    /// </summary>
    private void NotifySaveSuccess()
    {
        var title = $"{Node.Type} Saved";
        var message = $"{Node.Name} was saved successfully @ {DateTime.Now.ToShortTimeString()}";
        var notification = new Notification(title, message, NotificationType.Success);
        Notifier.Notify(notification);
    }

    /// <summary>
    /// Sends a notification to the UI that the node failed to save.
    /// </summary>
    private void NotifySaveFailed(IResultBase result)
    {
        const string title = "Saved Failed";
        var message = $"{Node.Name} failed to save @ {DateTime.Now.ToShortTimeString()} {result.Reasons}";
        var notification = new Notification(title, message, NotificationType.Error);
        Notifier.Notify(notification);
    }
}