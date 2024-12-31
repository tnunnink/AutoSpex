using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using AutoSpex.Client.Services;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentResults;

namespace AutoSpex.Client.Shared;

public abstract partial class DetailPageModel(string? title) : PageViewModel(title),
    IRecipient<Observer.Renamed>,
    IRecipient<Observer.Deleted>,
    IRecipient<NavigationRequest>
{
    public override bool IsChanged => base.IsChanged || Pages.Any(t => t.IsChanged);
    public override bool IsErrored => base.IsErrored || Pages.Any(t => t.IsErrored);
    public ObservableCollection<PageViewModel> Pages { get; } = [];

    [ObservableProperty] private PageViewModel? _currentPage;

    /// <summary>
    /// When this node page is closed and deactivated we also want to close any child tab pages.
    /// </summary>
    protected override void OnDeactivated()
    {
        base.OnDeactivated();

        foreach (var tab in Pages.ToList())
            Navigator.Close(tab);
    }

    /// <inheritdoc />
    /// <remarks>
    /// When a node page is loaded, it will forward the call to its child tabs to be loaded.
    /// </remarks>
    public override async Task Load()
    {
        await NavigatePages();
        Dispatcher.UIThread.Invoke(() => SaveCommand.NotifyCanExecuteChanged());
    }

    /// <inheritdoc />
    /// <remarks>
    /// When a node page is saved, it will forward the call to its child tabs to be saved.
    /// </remarks>
    public override async Task<Result> Save(Result? result = default)
    {
        result ??= Result.Ok();

        if (Notifier.ShowIfFailed(result, $"Failed to save {Title}"))
            return result;

        foreach (var tab in Pages)
        {
            var saved = await tab.Save();
            if (Notifier.ShowIfFailed(result, $"Failed to save {Title}"))
                return saved;
        }

        Notifier.ShowSuccess($"{Title} Saved", $"{Icon} was saved successfully @ {DateTime.Now.ToShortTimeString()}");
        AcceptChanges();
        return result;
    }

    /// <summary>
    /// A command to close the current detail page.
    /// </summary>
    /// <returns>The <see cref="Task"/> which can await the flag indicating whether the page was closed or not.</returns>
    /// <remarks>
    /// This will first check for changes, and if any exist and the AlwaysDiscardChanges is not enabled, will
    /// prompt the user whether they want to save, cancel, or discard changes. If they select an option which results
    /// in the page closing, this method will return <c>true</c>, otherwise it will return <c>false</c>.
    /// </remarks>
    /// 
    [RelayCommand]
    public async Task<bool> Close()
    {
        var discard = Settings.App.AlwaysDiscardChanges;
        if (discard || !IsChanged)
        {
            Navigator.Close(this);
            return true;
        }

        var answer = await Prompter.PromptSave(Title);
        switch (answer)
        {
            case "Save":
                await Save();
                break;
            case "Cancel":
                return false;
        }

        Navigator.Close(this);
        return true;
    }

    /// <summary>
    /// A command to force close the page regardless of the state. This would mean discarding any current changes.
    /// This command simply uses the <see cref="Navigator"/> to issue the <see cref="Navigator.Close"/> which
    /// other pages should subscribe to if they are expected to close the pages they contain.
    /// </summary>
    [RelayCommand]
    private void ForceClose()
    {
        Navigator.Close(this);
    }

    /// <summary>
    /// Close this node if was deleted.
    /// </summary>
    public virtual void Receive(Observer.Deleted message)
    {
        if (!Route.Contains(message.Observer.Id.ToString())) return;
        ForceClose();
    }

    /// <summary>
    /// When the observer this detail page contains has a name change then notify the title property.
    /// </summary>
    public virtual void Receive(Observer.Renamed message)
    {
        if (!Route.Contains(message.Observer.Id.ToString())) return;
        Title = message.Observer.Name;
    }

    /// <summary>
    /// Handles the navigation request for this node detail page. Only handle tabs that start with the same route,
    /// meaning it is a child tab of this page.
    /// </summary>
    /// <param name="message">The navigation request message.</param> 
    public void Receive(NavigationRequest message)
    {
        var page = message.Page;
        if (page.Route == Route) return;
        if (!page.Route.StartsWith(Route)) return;

        switch (message.Action)
        {
            case NavigationAction.Open:
                if (TryOpenExisting()) break;
                Pages.Add(page);
                Track(page);
                break;
            case NavigationAction.Close:
                Pages.Remove(page);
                Forget(page);
                break;
            case NavigationAction.Replace:
                break;
            default:
                throw new ArgumentOutOfRangeException($"Navigation action {message.Action} not supported");
        }

        return;

        bool TryOpenExisting()
        {
            var existing = Pages.SingleOrDefault(p => p.Route == page.Route);
            
            if (existing is not null)
            {
                CurrentPage = existing;
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Handles requesting navigation of any child tabs for this node page.
    /// </summary>
    protected virtual Task NavigatePages() => Task.CompletedTask;
    

    //We need to notify the SaveCommand when the IsChange property changes since that is what it is controlled on.
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName is nameof(IsChanged) or nameof(IsErrored))
        {
            Dispatcher.UIThread.Invoke(() => SaveCommand.NotifyCanExecuteChanged());
        }
    }
}