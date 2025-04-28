using System.Threading.Tasks;
using AutoSpex.Client.Shared;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using FluentResults;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class AppPageModel : PageViewModel, IRecipient<AppPageModel.OpenDrawerRequest>
{
    public Task<NavigationPageModel> Navigation => Navigator.Navigate<NavigationPageModel>();
    public Task<DetailsPageModel> Details => Navigator.Navigate<DetailsPageModel>();
    public Task<DrawerPageModel> Drawer => Navigator.Navigate<DrawerPageModel>();

    [ObservableProperty] private bool _isDrawerOpen;

    [RelayCommand]
    private Task OpenSettings()
    {
        return Prompter.Show(() => new SettingsPageModel());
    }

    [RelayCommand]
    private async Task SaveAll()
    {
        var command = Messenger.Send(new SaveAllRequest()).Response;
        await command.ExecuteAsync(null);
    }

    [RelayCommand]
    private async Task SaveSelected()
    {
        var command = Messenger.Send(new SaveSelectedRequest()).Response;
        await command.ExecuteAsync(null);
    }

    [RelayCommand]
    private async Task CloseAllTabs()
    {
        var command = Messenger.Send(new CloseAllTabsRequest()).Response;
        await command.ExecuteAsync(null);
    }

    public class SaveAllRequest : RequestMessage<IAsyncRelayCommand>;

    public class SaveSelectedRequest : RequestMessage<IAsyncRelayCommand>;

    public class CloseAllTabsRequest : RequestMessage<IAsyncRelayCommand>;

    public record OpenDrawerRequest;

    /// <summary>
    /// Handle the request to open the drawer view of the application.
    /// </summary>
    public void Receive(OpenDrawerRequest message)
    {
        IsDrawerOpen = true;
    }
}