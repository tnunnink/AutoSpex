using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
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
    private async Task CreateSpec()
    {
        var spec = await Prompter.Show<NodeObserver?>(() => new AddSpecPageModel());
        if (spec is null) return;
        await Navigator.Navigate(spec);
    }

    public void Receive(NavigationRequest message)
    {
        if (message.Page is not DetailPageModel detail) return;

        if (message.Action == NavigationAction.Close)
        {
            CloseTab(detail);
            return;
        }

        if (SelectExistingIfOpen(detail)) return;

        if (message.Action == NavigationAction.Replace)
        {
            ShowOrReplace(detail);
            return;
        }

        OpenTab(detail);
    }

    private void OpenTab(DetailPageModel page)
    {
        Pages.Add(page);
        Selected = page;
    }

    private void CloseTab(DetailPageModel page)
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