using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Pages.Specs;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentResults;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages.Projects;

[UsedImplicitly]
public partial class DetailsPageModel : PageViewModel, IRecipient<NavigationRequest<NodePageModel>>
{
    [ObservableProperty] private ObservableCollection<PageViewModel> _pages = [];

    [ObservableProperty] private PageViewModel? _selected;

    [RelayCommand]
    private Task AddSpec()
    {
        var node = Node.NewSpec();
        var newTab = new KeyValuePair<string, object>("NewTab", true);
        return Navigator.Navigate(() => new NodePageModel(node), newTab);
    }

    public void Receive(NavigationRequest<NodePageModel> message)
    {
        if (message.Parameters.TryGetValue("NewTab", out var newTab) && newTab is true)
        {
            ShowInNewTab(message.Page);
            message.Reply(Result.Ok());
            return;
        }

        var existing = Pages.SingleOrDefault(x => x.Route == message.Page.Route);
        if (existing is not null)
        {
            Selected = existing;
            message.Reply(Result.Ok());
            return;
        }

        ShowOrReplace(message.Page);
        message.Reply(Result.Ok());
    }

    private void ShowOrReplace(PageViewModel page)
    {
        //Try to replace an existing page that has not been changed.
        for (var i = 0; i < Pages.Count; i++)
        {
            if (Pages[i].IsChanged) continue;
            var replaceable = Pages[i];
            replaceable.IsActive = false;
            Pages[i] = page;
            Selected = page;
            return;
        }

        //No pages are open, so add and focus.
        Pages.Add(page);
        Selected = page;
    }

    private void ShowInNewTab(PageViewModel page)
    {
        Pages.Add(page);
        Selected = page;
    }
}