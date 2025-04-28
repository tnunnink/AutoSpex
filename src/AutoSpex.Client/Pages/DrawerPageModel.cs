using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class DrawerPageModel : PageViewModel, IRecipient<NavigationRequest>
{
    [ObservableProperty] private ObservableCollection<DetailPageModel> _pages = [];

    [ObservableProperty] private DetailPageModel? _selected;

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

    /// <inheritdoc />
    protected override void OnDeactivated()
    {
        foreach (var page in Pages.ToList())
        {
            Navigator.Close(page);
        }

        base.OnDeactivated();
    }

    /// <summary>
    /// Handle the reception of the navigation request for a detail page model object.
    /// Either open (add to pages) or close (remove from pages) depending on the action.
    /// Also don't open duplicate detail pages.
    /// </summary>
    public void Receive(NavigationRequest message)
    {
        if (message.Page is not DetailPageModel page) return;
        if (!IsExpectedPage(page)) return;

        if (message.Action == NavigationAction.Close)
        {
            ClosePage(page);
            return;
        }

        if (ShowIfOpen(page)) return;

        if (message.Action == NavigationAction.Replace)
        {
            ShowOrReplace(page);
            return;
        }

        OpenPage(page);
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

    private bool ShowIfOpen(PageViewModel page)
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
    
    /// <summary>
    /// Indicates the page is one that this page should contain open/close as tabs in the tab strip.
    /// </summary>
    private static bool IsExpectedPage(DetailPageModel page) => page is RunnerPageModel;
}