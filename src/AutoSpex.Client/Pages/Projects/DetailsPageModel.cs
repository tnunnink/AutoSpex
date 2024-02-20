using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages.Projects;

[UsedImplicitly]
public partial class DetailsPageModel : PageViewModel, IRecipient<NavigationRequest>
{
    [ObservableProperty] private ObservableCollection<DetailPageModel> _pages = [];

    [ObservableProperty] private PageViewModel? _selected;

    [RelayCommand]
    private async Task AddSpec()
    {
        var node = new NodeObserver(Node.NewSpec()) {FocusName = true};
        await Navigator.Navigate(node);
    }

    public void Receive(NavigationRequest message)
    {
        if (message.Page is not DetailPageModel details) return;

        if (message.Action == NavigationAction.Close)
        {
            CloseTab(details);
            return;
        }

        if (SelectExistingIfOpen(details)) return;

        if (message.Action == NavigationAction.Open)
        {
            ShowInNewTab(details);
            return;
        }

        ShowOrReplace(details);
    }

    private bool SelectExistingIfOpen(PageViewModel page)
    {
        var existing = Pages.SingleOrDefault(x => x.Route == page.Route);
        if (existing is null) return false;
        Selected = existing;
        return true;
    }

    private void ShowInNewTab(DetailPageModel page)
    {
        Pages.Add(page);
        Selected = page;
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

    private void CloseTab(DetailPageModel page)
    {
        Pages.Remove(page);

        if (Selected is not null && Selected == page)
            Selected = Pages.FirstOrDefault();
    }
}