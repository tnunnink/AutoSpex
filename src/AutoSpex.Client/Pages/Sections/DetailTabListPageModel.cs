using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Pages;

public partial class DetailTabListPageModel : PageViewModel, IRecipient<NavigationRequest>
{
    private readonly List<DetailPageModel> _tabs;

    public DetailTabListPageModel(List<DetailPageModel> tabs)
    {
        _tabs = tabs;
        UpdateTabs();
    }

    public override bool KeepAlive => false;
    public ObservableCollection<DetailPageModel> Tabs { get; } = [];

    /// <summary>
    /// If the page is closed we need to respond by removing it from the local collection and refreshing the UI.
    /// </summary>
    public void Receive(NavigationRequest message)
    {
        if (message.Action != NavigationAction.Close) return;
        if (message.Page is not DetailPageModel page) return;

        var result = _tabs.Remove(page);

        if (result)
        {
            UpdateTabs(Filter);
        }
    }

    /// <summary>
    /// Trigger collection filtering and update when filter text changes.
    /// </summary>
    protected override void FilterChanged(string? filter)
    {
        UpdateTabs(filter);
    }

    /// <summary>
    /// Filters and updates the local <see cref="Tabs"/> collection.
    /// </summary>
    private void UpdateTabs(string? filter = default)
    {
        var filtered = _tabs.Where(t => t.Title.Satisfies(filter));
        Tabs.Refresh(filtered);
    }
}