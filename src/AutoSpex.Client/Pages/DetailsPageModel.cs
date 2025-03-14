﻿using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentResults;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class DetailsPageModel : PageViewModel, IRecipient<NavigationRequest>
{
    [ObservableProperty] private ObservableCollection<DetailPageModel> _pages = [];

    [ObservableProperty] private DetailPageModel? _selected;

    public Task<DetailTabListPageModel> TabList => Navigator.Navigate(() => new DetailTabListPageModel(Pages.ToList()));

    protected override void OnDeactivated()
    {
        foreach (var page in Pages.ToList())
        {
            Navigator.Close(page);
        }

        base.OnDeactivated();
    }

    /// <summary>
    /// Command to quickly create a new collection node.
    /// </summary>
    [RelayCommand]
    private async Task NewCollection()
    {
        var node = Node.NewCollection();

        var result = await Mediator.Send(new CreateNode(node));
        if (Notifier.ShowIfFailed(result, "Failed to create new collection. See notifications for details.")) return;

        var observer = new NodeObserver(node) { IsNew = true };
        Messenger.Send(new Observer.Created<NodeObserver>(observer));
        await Navigator.Navigate(observer);
    }

    /// <summary>
    /// Command to quickly create a new spec node and open the details for the user to configure.
    /// This will be a virtual node until the user attempts to save it, in which case they should get prompted where
    /// to save it.
    /// </summary>
    [RelayCommand]
    private async Task NewSpec()
    {
        var node = Node.NewSpec();
        var observer = new NodeObserver(node) { IsNew = true };
        await Navigator.Navigate(observer);
    }

    /// <summary>
    /// Command to quickly create a new source and open the details for in the detail view.
    /// </summary>
    [RelayCommand]
    private async Task NewSource()
    {
        var source = await Prompter.Show<SourceObserver?>(() => new NewSourcePageModel());
        if (source is null) return;

        var result = await Mediator.Send(new CreateSource(source));
        if (Notifier.ShowIfFailed(result, "Failed to create new source. See notifications for details.")) return;

        var observer = new SourceObserver(source);
        Messenger.Send(new Observer.Created<SourceObserver>(observer));
    }

    /// <summary>
    /// Command to close the provided tab page from the details view.
    /// </summary>
    [RelayCommand]
    private static async Task CloseTab(DetailPageModel? page)
    {
        if (page is null) return;
        await page.Close();
    }

    /// <summary>
    /// Command to close all current open tabs from teh details view.
    /// </summary>
    [RelayCommand]
    private async Task CloseAllTabs()
    {
        var pages = Pages.ToList();

        foreach (var page in pages)
        {
            await page.Close();
        }

        pages.Clear();
    }

    [RelayCommand]
    private async Task CloseOtherTabs(PageViewModel? page)
    {
        if (page is null) return;

        var pages = Pages.Where(p => p != page).ToList();

        foreach (var closable in pages)
        {
            await closable.Close();
        }

        pages.Clear();
    }

    [RelayCommand]
    private async Task CloseRightTabs(DetailPageModel? page)
    {
        if (page is null) return;

        var pages = Pages.ToList();
        var start = pages.IndexOf(page) + 1;

        for (var i = start; i < pages.Count; i++)
        {
            var closable = pages[i];
            await closable.Close();
        }
    }

    [RelayCommand]
    private async Task CloseLeftTabs(DetailPageModel? page)
    {
        if (page is null) return;

        var pages = Pages.ToList();
        var start = pages.IndexOf(page) - 1;

        for (var i = start; i >= 0; i--)
        {
            var closable = pages[i];
            await closable.Close();
        }
    }

    [RelayCommand]
    private void ForceCloseAllTabs()
    {
        var pages = Pages.ToList();

        foreach (var page in pages)
        {
            page.ForceCloseCommand.Execute(null);
        }

        pages.Clear();
    }

    [RelayCommand]
    private async Task SaveSelected()
    {
        if (Selected is null || !Selected.IsChanged) return;
        await Selected.Save();
    }

    [RelayCommand]
    private async Task SaveAll()
    {
        foreach (var page in Pages.Where(p => p.IsChanged))
        {
            await page.Save();
        }
    }

    /// <summary>
    /// Handle the reception of the navigation request for a detail page model object. Either open (add to pages) or
    /// close (remove from pages) depending on the action. Also don't open duplicate detail pages.
    /// </summary>
    public void Receive(NavigationRequest message)
    {
        if (message.Page is not DetailPageModel detail) return;

        if (message.Action == NavigationAction.Close)
        {
            ClosePage(detail);
            return;
        }

        if (SelectExistingIfOpen(detail)) return;

        if (message.Action == NavigationAction.Replace)
        {
            ShowOrReplace(detail);
            return;
        }

        OpenPage(detail);
    }

    private void OpenPage(DetailPageModel page)
    {
        Pages.Add(page);
        Selected = page;
    }

    private void ClosePage(DetailPageModel page)
    {
        Pages.Remove(page);

        if (Selected is not null && Selected == page)
            Selected = Pages.FirstOrDefault();
    }

    private bool SelectExistingIfOpen(PageViewModel page)
    {
        var existing = Pages.SingleOrDefault(x => x.Route == page.Route);
        if (existing is null) return false;
        Selected = existing;
        return true;
    }

    private void ShowOrReplace(DetailPageModel page)
    {
        //Try to replace an existing page that has not been changed.
        for (var i = 0; i < Pages.Count; i++)
        {
            if (Pages[i].IsChanged) continue;
            var closable = Pages[i];
            closable.IsActive = false;
            Pages[i] = page;
            Selected = page;
            return;
        }

        //No pages are open, so add and focus.
        Pages.Add(page);
        Selected = page;
    }
}