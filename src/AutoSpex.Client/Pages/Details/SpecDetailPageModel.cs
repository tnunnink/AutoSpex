using System;
using System.Linq;
using System.Threading.Tasks;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Persistence;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentResults;

namespace AutoSpex.Client.Pages;

public partial class SpecDetailPageModel(NodeObserver node) : DetailPageModel(node.Name)
{
    public override string Route => $"Spec/{Node.Id}";
    public override string Icon => Node.Type.Name;
    public NodeObserver Node { get; } = node;

    [ObservableProperty] private CriteriaPageModel? _criteriaPage;

    [ObservableProperty] private SpecRunnerPageModel? _runnerPage;

    [ObservableProperty] private bool _showRunner;


    /// <inheritdoc />
    public override async Task Load()
    {
        await NavigateContent();

        //New specs need to enable the save button by default.
        Dispatcher.UIThread.Invoke(() => SaveCommand.NotifyCanExecuteChanged());

        //Any time a node is open locate it in the navigation tree.
        Messenger.Send(new NodeObserver.ExpandTo(Node.Id));
    }

    /// <inheritdoc />
    /// <remarks>
    /// Check if the node is "virtual" (has no parent) and therfore not saved to the database
    /// (this only applies to spec or container nodes). Create node if virtual. Otherwise, continue saving.
    /// </remarks>
    public override async Task<Result> Save(Result? result = default)
    {
        if (CriteriaPage is null) return Result.Fail($"Failed to load spec config for {Node.Name}");

        var errors = Validate().ToList();
        if (errors.Count > 0)
        {
            Notifier.ShowError($"Failed to save {Title}", $"{errors.FirstOrDefault()?.ErrorMessage}");
            return Result.Fail("Failed to save page due to validation errors.");
        }

        var created = Node.IsVirtual ? await CreateNode() : Result.Ok();
        var saved = await CriteriaPage.Save();
        result = Result.Merge(created, saved);
        return await base.Save(result);
    }

    /// <inheritdoc />
    /// <remarks>If this node is virtual then we need to enable the save command.</remarks>
    public override bool CanSave()
    {
        return base.CanSave() || Node.ParentId == Guid.Empty;
    }

    /// <summary>
    /// Uses the current test runner page to execeute this specification.
    /// Opens the runner drawer if not alread open.
    /// </summary>
    [RelayCommand]
    private async Task Run()
    {
        if (RunnerPage is null) return;
        await RunnerPage.Run(CriteriaPage?.Spec?.Model);
        ShowRunner = true;
    }

    /// <inheritdoc />
    protected override async Task NavigateContent()
    {
        CriteriaPage = await Navigator.Navigate(() => new CriteriaPageModel(Node));
        Track(CriteriaPage);

        RunnerPage = await Navigator.Navigate(() => new SpecRunnerPageModel(Node));
    }

    protected override void OnDeactivated()
    {
        if (CriteriaPage is not null) Navigator.Close(CriteriaPage);
        if (RunnerPage is not null) Navigator.Close(RunnerPage);
        base.OnDeactivated();
    }

    /// <summary>
    /// Creates this node in the database by prompting the user to select a parent container, and then sending the
    /// CreateNode request with the update node parent id. This should only be used for virtual nodes that have not
    /// yet been saved to the database.
    /// </summary>
    private async Task<Result> CreateNode()
    {
        var parent = await Prompter.Show<NodeObserver?>(() => new SelectContainerPageModel { ButtonText = "Save" });

        if (parent is null)
            return Result.Fail("Node requires parent to be saved.");

        parent.Nodes.Add(Node);

        var result = await Mediator.Send(new CreateNode(Node));

        if (result.IsSuccess)
        {
            Messenger.Send(new Observer.Created<NodeObserver>(Node));
        }

        return result;
    }
}