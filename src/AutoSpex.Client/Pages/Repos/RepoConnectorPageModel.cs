using System.Threading.Tasks;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class RepoConnectorPageModel : PageViewModel, IRecipient<RepoObserver.SetConnected>
{
    [ObservableProperty] private RepoObserver? _repo;
    public Task<RepoListPageModel> RepoList => Navigator.Navigate<RepoListPageModel>();

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