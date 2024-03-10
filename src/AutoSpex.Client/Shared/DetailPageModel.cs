using System;
using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Shared;

public abstract partial class DetailPageModel : PageViewModel, IRecipient<NavigationRequest>
{
    public ObservableCollection<PageViewModel> Tabs { get; } = [];
    public ObservableCollection<PageViewModel> Details { get; } = [];

    [ObservableProperty] private bool _isDetailDrawerOpen;

    [ObservableProperty] private PageViewModel? _selectedDetailPage;

    protected override void OnDeactivated()
    {
        foreach (var page in Tabs.ToList())
            Navigator.Close(page);

        foreach (var page in Details.ToList())
            Navigator.Close(page);

        base.OnDeactivated();
    }

    public virtual void Receive(NavigationRequest message)
    {
    }

    /// <summary>
    /// A command to control the view of a child detail page which can be shown in an inner side drawer view.
    /// </summary>
    /// <param name="pageTitle">The title of the page to toggle into/out of the view of the page.</param>
    [RelayCommand]
    private void ShowDetailPage(string pageTitle)
    {
        //If already open then toggle closed.
        if (IsDetailDrawerOpen && SelectedDetailPage?.Route.Contains(pageTitle) is true)
        {
            SelectedDetailPage = null;
            IsDetailDrawerOpen = false;
            return;
        }

        //Otherwise set the selected detail page to the page with matching title name.
        var page = Details.FirstOrDefault(p => p.Route.Contains(pageTitle));
        SelectedDetailPage = page;
        IsDetailDrawerOpen = true;
    }

    /// <summary>
    /// Navigates the provided tab page into (or out of) the <see cref="Tabs"/> collection for the page.
    /// </summary>
    protected void NavigateTabPage(PageViewModel page, NavigationAction action)
    {
        switch (action)
        {
            case NavigationAction.Replace:
            case NavigationAction.Open:
                if (Tabs.Contains(page)) return;
                Tabs.Add(page);
                break;
            case NavigationAction.Close:
                Tabs.Remove(page);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action), action, null);
        }
    }

    /// <summary>
    /// Navigates the provided detail page into (or out of) the <see cref="Details"/> collection for the page.
    /// </summary>
    protected void NavigateDetailPage(PageViewModel page, NavigationAction action)
    {
        switch (action)
        {
            case NavigationAction.Replace:
            case NavigationAction.Open:
                if (Details.Contains(page)) return;
                Details.Add(page);
                break;
            case NavigationAction.Close:
                Details.Remove(page);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action), action, null);
        }
    }
}