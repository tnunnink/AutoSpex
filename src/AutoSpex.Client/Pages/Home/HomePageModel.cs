using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages.Home;

[UsedImplicitly]
public partial class HomePageModel : PageViewModel, IRecipient<NavigationRequest>
{
    [ObservableProperty] private PageViewModel? _page;

    [ObservableProperty] private string _menu = string.Empty;

    partial void OnMenuChanged(string value)
    {
        switch (value)
        {
            case "Projects":
                Navigator.Navigate<HomeProjectPageModel>();
                break;
            case "Release Notes":
                /*Navigator.Navigate<HomeProjectPageModel, HomePageModel>();*/
                break;
            case "Settings":
                /*Navigator.Navigate<HomeProjectPageModel, HomePageModel>();*/
                break;
        }
    }

    public void Receive(NavigationRequest message)
    {
        if (message.Page is not HomeProjectPageModel and not CreateProjectPageModel) return;

        if (Page is not null)
            Page.IsActive = false;

        Page = message.Page;
    }
}