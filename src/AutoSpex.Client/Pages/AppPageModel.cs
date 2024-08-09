using System;
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
public partial class AppPageModel : PageViewModel, IRecipient<NavigationRequest>
{
    [ObservableProperty] private PageViewModel? _navigationPage;

    [ObservableProperty] private PageViewModel? _detailsPage;

    /// <inheritdoc />
    public override async Task Load()
    {
        await Navigator.Navigate<NavigationPageModel>();
        await Navigator.Navigate<DetailsPageModel>();
    }

    /// <inheritdoc />
    protected override void OnDeactivated()
    {
        if (NavigationPage is not null)
            Navigator.Close(NavigationPage);

        if (DetailsPage is not null)
            Navigator.Close(DetailsPage);

        base.OnDeactivated();
    }

    /// <summary>
    /// Handles the navigation requests for this main project page.
    /// </summary>
    public void Receive(NavigationRequest message)
    {
        switch (message.Action)
        {
            case NavigationAction.Open:
                OpenPage(message);
                break;
            case NavigationAction.Close:
                ClosePage(message);
                break;
            case NavigationAction.Replace:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(message), "Navigation action out of expected range");
        }
    }

    /// <summary>
    /// Opens the requested page depending on the model that is passed in.
    /// </summary>
    private void OpenPage(NavigationRequest message)
    {
        switch (message.Page)
        {
            case NavigationPageModel:
                NavigationPage = message.Page;
                break;
            case DetailsPageModel:
                DetailsPage = message.Page;
                break;
        }
    }

    /// <summary>
    /// Closes the requested page depending on the model that is passed in.
    /// </summary>
    private void ClosePage(NavigationRequest message)
    {
        switch (message.Page)
        {
            case NavigationPageModel:
                NavigationPage = null;
                break;
            case DetailsPageModel:
                DetailsPage = null;
                break;
        }
    }
}