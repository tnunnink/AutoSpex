using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class RepoConfigPageModel : PageViewModel, IRecipient<RepoObserver.SetConnected>
{
    [ObservableProperty] private RepoObserver? _repo;

    /// <inheritdoc />
    protected override void FilterChanged(string? filter)
    {
        Repo?.Sources.Filter(filter);
        Repo?.Refresh();
    }
    
    /// <summary>
    /// Command to select folder location from disc send connection requested to database.
    /// Once connected, we will update the source list
    /// </summary>
    [RelayCommand]
    private async Task OpenRepo()
    {
        var location = await Shell.StorageProvider.SelectLocation("Select Repository Location");
        if (location is null) return;

        var repo = new RepoObserver(new Repo(location));
        await repo.Connect();
    }

    /// <summary>
    /// Reset when the connect repo is changed.
    /// </summary>
    public void Receive(RepoObserver.SetConnected message)
    {
        //Ensures change of the observable property which only updates based on equality which we implement using Ids.
        Repo = null;
        Repo = message.Repo;
    }
}