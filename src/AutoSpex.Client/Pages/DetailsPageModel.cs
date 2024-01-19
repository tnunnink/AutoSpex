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
public partial class DetailsPageModel : PageViewModel, IRecipient<NavigationRequest>
{
    [ObservableProperty] private ObservableCollection<PageViewModel> _pages = [];

    [ObservableProperty] private PageViewModel? _selected;

    [RelayCommand]
    private void CloseNode(PageViewModel details)
    {
        if (details.IsChanged)
        {
            //todo prompt user.
        }

        Pages.Remove(details);
    }

    protected override void OnActivated()
    {
        Messenger.Register<NavigationRequest, string>(this, nameof(DetailsPageModel));
    }
    
    public void Receive(NavigationRequest message)
    {
        if (message.Parameters.TryGetValue("NewTab", out var newTab) && newTab is bool flag)
        {
            ShowNode(message.Page, flag);
        }
        
        ShowNode(message.Page, false);
    }

    private void ShowNode(PageViewModel page, bool newTab)
    {
        //If this page is already open the simply select it, bringing it into focus.
        var existing = Pages.SingleOrDefault(x => x.Route == page.Route);
        if (existing is not null)
        {
            Selected = page;
            return;
        }

        //If this is supposed to be in a new tab, add the page and select to bring into focus.
        if (newTab)
        {
            Pages.Add(page);
            Selected = page;
            return;
        }

        //From here try to replace an existing page that has not been changed.
        for (var i = 0; i < Pages.Count; i++)
        {
            if (Pages[i].IsChanged) continue;
            Pages[i] = page;
            Selected = Pages[i];
            return;
        }

        //No pages are open, so add and focus.
        Pages.Add(page);
        Selected = page;
    }
}