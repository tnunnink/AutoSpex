using System;
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
    public Task<SourceTargetPageModel> SourceSelector => Navigator.Navigate<SourceTargetPageModel>();

    #region Commands

    /// <summary>
    /// Command to quickly create a new collection node.
    /// </summary>
    [RelayCommand]
    private async Task AddCollection()
    {
        var node = Node.NewCollection();

        var result = await Mediator.Send(new CreateNode(node));
        if (result.IsFailed) return;

        var observer = new NodeObserver(node) { IsNew = true };
        Messenger.Send(new Observer.Created<NodeObserver>(observer));
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
    /// Command to quickly create a new source and open the details for in the detail view.
    /// </summary>
    [RelayCommand]
    private async Task AddSource()
    {
        var source = await Prompter.Show<SourceObserver?>(() => new NewSourcePageModel());
        if (source is null) return;

        var result = await Mediator.Send(new CreateSource(source));
        if (Notifier.ShowIfFailed(result, "Failed to create new source. See notifications for details.")) return;

        var observer = new SourceObserver(source);
        Messenger.Send(new Observer.Created<SourceObserver>(observer));
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
        var exists = await Mediator.Send(new ContainsNode(package.Collection.Name, NodeType.Collection));

        if (exists)
        {
            action = await Prompter.Show<ImportAction?>(() => new ImportConflictPageModel(package));
        }

        if (action is null || action == ImportAction.Cancel) return;

        var import = await Mediator.Send(new ImportNode(package, action));
        if (Notifier.ShowIfFailed(import)) return;

        Messenger.Send(new Observer.Created<NodeObserver>(new NodeObserver(import.Value)));

        Notifier.ShowSuccess(
            "Import request complete",
            $"Import of {import.Value.Name} completed successfully @ {DateTime.Now}"
        );
    }

    [RelayCommand]
    private async Task OpenSearch()
    {
        await Prompter.Show(() => new SearchPageModel());
    }

    [RelayCommand]
    private async Task OpenSources()
    {
        await Navigator.Navigate<SourceManagerPageModel>();
    }

    [RelayCommand]
    private async Task OpenHistory()
    {
        await Navigator.Navigate<HistoryDetailPageModel>();
    }

    [RelayCommand]
    private Task OpenSettings()
    {
        return Prompter.Show(() => new SettingsPageModel());
    }

    #endregion
}