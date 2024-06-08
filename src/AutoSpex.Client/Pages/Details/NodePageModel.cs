using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentResults;

namespace AutoSpex.Client.Pages;

public abstract partial class NodePageModel : DetailPageModel,
    IRecipient<ProjectObserver.Changed>,
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
    public ObservableCollection<PageViewModel> Tabs { get; } = [];

    [ObservableProperty] private PageViewModel? _tab;

    /// <summary>
    /// When a node page is loaded, it will forward the call to its child tabs to be loaded.
    /// </summary>
    public override async Task Load()
    {
        await NavigateTabs();
        Dispatcher.UIThread.Invoke(() => SaveCommand.NotifyCanExecuteChanged());
    }

    /// <summary>
    /// Executes the run of the contained node. If this node is a spec or source (or one of its containers) then this
    /// command will navigate a new in memory run page with this node and its descendants automatically added. If this
    /// is a run node, then this command will navigate the configured run into the Runner page, which will then be executed.
    /// </summary>
    /// <returns></returns>
    [RelayCommand(CanExecute = nameof(CanRun))]
    protected virtual Task Run() => Task.CompletedTask;

    /// <summary>
    /// Determines if the run command can be executed for this node page model.
    /// </summary>
    /// <returns><c>true</c> if the command can be executed, otherwise <c>false</c>.</returns>
    protected virtual bool CanRun() => true;

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
    /// If the project has detected a change refresh this node by recalling its load method. 
    /// </summary>
    public async void Receive(ProjectObserver.Changed message)
    {
        //todo technically there is nothing checking that the node actually still exists here. Maybe this is where we would handle that and navigate the node not found page in.

        //Since this view model is just a container for others, we just want to iterate the tabs can call their reload.
        foreach (var tab in Tabs)
        {
            await tab.Load();
        }
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
        var page = message.Page;
        if (page.Route == Route) return;
        if (!page.Route.StartsWith(Route)) return;

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
            case NavigationAction.Replace:
                var index = Tabs.IndexOf(page);
                if (index < 0) break;
                Tabs[index] = message.Page;
                break;
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