using System.Threading.Tasks;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class RepoConnectorPageModel : PageViewModel,
    IRecipient<RepoObserver.SetConnected>
{
    [ObservableProperty] private RepoObserver? _repo;
    public Task<RepoListPageModel> RepoList => Navigator.Navigate<RepoListPageModel>();

    /// <inheritdoc />
    public override async Task Load()
    {
        var result = await Mediator.Send(new GetLastConnectedRepo());
        if (result.IsFailed) return;

        //Create the observer instance and call connect.
        //This will be received here and in config page to update the local observer instance.
        var repo = new RepoObserver(result.Value);
        await repo.Connect();
    }

    /// <summary>
    /// When a repo is connected update the local instance to reflect the change in the UI.
    /// </summary>
    public void Receive(RepoObserver.SetConnected message)
    {
        //Ensures change of the observable property which only updates based on equality which we implement using Ids.
        Repo = null;
        Repo = message.Repo;
    }
}