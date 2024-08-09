using System.Threading.Tasks;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class NavigationPageModel : PageViewModel
{
    public Task<NodeTreePageModel> NodeTree => Navigator.Navigate<NodeTreePageModel>();
    public Task<EnvironmentListPageModel> EnvironmentList => Navigator.Navigate<EnvironmentListPageModel>();

    #region Commands

    [RelayCommand]
    private async Task NewCollection()
    {
        var node = Node.NewCollection();

        var result = await Mediator.Send(new CreateNode(node));
        if (result.IsFailed) return;

        var observer = new NodeObserver(node) { IsNew = true };
        Messenger.Send(new Observer.Created(observer));
        await Navigator.Navigate(observer);
    }

    [RelayCommand]
    private async Task AddSpec()
    {
        var node = Node.NewSpec();
        var observer = new NodeObserver(node) { IsNew = true };
        await Navigator.Navigate(observer);
    }

    [RelayCommand]
    private async Task AddEnvironment()
    {
        var environment = new Environment();
        var result = await Mediator.Send(new CreateEnvironment(environment));
        if (result.IsFailed) return;

        var observer = new EnvironmentObserver(environment) { IsNew = true };
        Messenger.Send(new Observer.Created(observer));

        await Navigator.Navigate(observer);
    }

    [RelayCommand]
    private Task Import()
    {
        //todo
        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task Search()
    {
        //todo
        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task OpenExplorer()
    {
        return Prompter.Show(() => new SourceExplorerPageModel());
    }

    [RelayCommand]
    private Task OpenHistory()
    {
        //todo
        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task OpenVariables()
    {
        //todo
        return Task.CompletedTask;
    }

    #endregion
}