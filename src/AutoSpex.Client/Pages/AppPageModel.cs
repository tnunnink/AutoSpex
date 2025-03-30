using System;
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
    [ObservableProperty] private NavigationPageModel? _navigationPage;

    [ObservableProperty] private DetailsPageModel? _detailsPage;

    [ObservableProperty] private RunnerPageModel? _runnerPage;

    [ObservableProperty] private bool _isRunnerDrawerOpen;


    /// <inheritdoc />
    public override async Task Load()
    {
        await Navigator.Navigate<NavigationPageModel>();
        await Navigator.Navigate<DetailsPageModel>();
        await Navigator.Navigate<RunnerPageModel>();
    }

    /// <inheritdoc />
    protected override void OnDeactivated()
    {
        if (NavigationPage is not null)
            Navigator.Close(NavigationPage);

        if (DetailsPage is not null)
            Navigator.Close(DetailsPage);

        if (RunnerPage is not null)
            Navigator.Close(RunnerPage);

        base.OnDeactivated();
    }

    [RelayCommand]
    private Task OpenSettings()
    {
        return Prompter.Show(() => new SettingsPageModel());
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
            case NavigationPageModel page:
                NavigationPage = page;
                break;
            case DetailsPageModel page:
                DetailsPage = page;
                break;
            case RunnerPageModel page:
                RunnerPage = page;
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
            case RunnerPageModel:
                RunnerPage = null;
                break;
        }
    }

    /*/// <summary>
    /// When a node is triggered to run from somewhere, open the runner drawer automatically to show the process.
    /// </summary>
    public void Receive(RunnerObserver.Run message)
    {
        IsRunnerDrawerOpen = true;
    }*/
}