using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class DetailsPageModel(string project) : PageViewModel, IRecipient<NavigationRequest>
{
    public override string Route => $"{project}/Details";
    public override bool IsChanged => Pages.Any(p => p.IsChanged);

    [ObservableProperty] private ObservableCollection<DetailPageModel> _pages = [];

    [ObservableProperty] private PageViewModel? _selected;

    protected override void OnDeactivated()
    {
        foreach (var page in Pages.ToList())
        {
            Navigator.Close(page);
        }

        base.OnDeactivated();
    }

    [RelayCommand]
    private async Task CreateNode(NodeType? feature)
    {
        if (feature is null) return;
        var node = await Prompter.Show<NodeObserver?>(() => new CreateNodePageModel(feature));
        if (node is null) return;
        await node.NavigateCommand.ExecuteAsync(null);
    }

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
    
    [RelayCommand]
    private static async Task CloseTab(DetailPageModel? page)
    {
        if (page is null) return;
        await page.Close(); //todo we might not need this command and just use the bound page/tab close command.
    }

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