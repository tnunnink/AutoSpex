using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.Input;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class RepoListPageModel : PageViewModel
{
    /// <inheritdoc />
    public override bool Reload => true;
    public ObserverCollection<Repo, RepoObserver> Recent { get; } = [];

    public override async Task Load()
    {
        var repos = await Mediator.Send(new ListRepos());
        Recent.Bind(repos.ToList(), r => new RepoObserver(r));
        RegisterDisposable(Recent);
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

    /// <inheritdoc />
    protected override void FilterChanged(string? filter)
    {
        Recent.Filter(filter);
    }
}