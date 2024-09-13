using System;
using System.Threading.Tasks;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;
using Environment = AutoSpex.Engine.Environment;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class NavigationPageModel : PageViewModel
{
    public Task<NodeTreePageModel> NodeTree => Navigator.Navigate<NodeTreePageModel>();
    public Task<EnvironmentListPageModel> EnvironmentList => Navigator.Navigate<EnvironmentListPageModel>();

    #region Commands

    /// <summary>
    /// Command to quickly create a new collection node.
    /// </summary>
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

    /// <summary>
    /// Command to quickly create a new spec node and open the details for the user to configure.
    /// This will be a virtual node until the user attempts to save it, in which case they should get prompted where
    /// to save it.
    /// </summary>
    [RelayCommand]
    private async Task AddSpec()
    {
        var node = Node.NewSpec();
        var observer = new NodeObserver(node) { IsNew = true };
        await Navigator.Navigate(observer);
    }

    /// <summary>
    /// Command to quickly create a new environment and open the details for in the detail view.
    /// </summary>
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

    /// <summary>
    /// Command to import new package into the application.
    /// </summary>
    [RelayCommand]
    private async Task Import()
    {
        var package = await Prompter.Show<Package?>(() => new OpenPackagePageModel());
        if (package is null) return;

        var action = ImportAction.None;
        var exists = await Mediator.Send(new HaveNode(package.Collection.Name, NodeType.Collection));

        if (exists)
        {
            action = await Prompter.Show<ImportAction?>(() => new ImportConflictPageModel(package));
        }

        if (action is null || action == ImportAction.Cancel) return;

        var import = await Mediator.Send(new ImportNode(package, action));
        if (Notifier.ShowIfFailed(import)) return;
        
        Messenger.Send(new Observer.Created(new NodeObserver(import.Value)));
        
        Notifier.ShowSuccess(
            "Import request complete",
            $"Import of {import.Value.Name} completed successfully @ {DateTime.Now}"
        );
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