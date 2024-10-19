﻿using System;
using System.Linq;
using System.Threading.Tasks;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentResults;

namespace AutoSpex.Client.Pages;

public partial class NodeDetailPageModel : DetailPageModel
{
    /// <inheritdoc/>
    public NodeDetailPageModel(NodeObserver node) : base(node.Name)
    {
        Node = node;
    }

    public override string Route => $"{Node.Type}/{Node.Id}";
    public override string Icon => Node.Type.Name;
    public NodeObserver Node { get; private set; }

    [ObservableProperty] private SpecRunnerPageModel? _runnerPage;


    /// <inheritdoc />
    public override async Task Load()
    {
        await LoadNode();
        await LoadRunner();
        await base.Load();
    }

    /// <inheritdoc />
    /// <remarks>
    /// Check if the node is "virtual" (has no parent) and therfore not saved to the database
    /// (this only applies to spec or container nodes). Create node if virtual. Otherwise, continue saving.
    /// </remarks>
    public override Task<Result> Save()
    {
        var errors = Validate().ToList();

        if (errors.Count == 0)
        {
            return Node.IsVirtual ? CreateNode() : SaveNode();
        }

        Notifier.ShowError($"Failed to save {Title}", $"{errors.FirstOrDefault()}");
        return Task.FromResult(Result.Fail("Failed to save page due to validation errors."));
    }

    /// <inheritdoc />
    /// <remarks>If this node is virtual then we need to enable the save command.</remarks>
    public override bool CanSave()
    {
        return base.CanSave() || (Node.Type != NodeType.Collection && Node.ParentId == Guid.Empty);
    }

    /// <summary>
    /// Runs this node against the target source. This involves loading the source and creating a new run
    /// instance with this node (and all descendant spec nodes if a container). Then navigates the run detail page into view
    /// which will execute the created run instance. 
    /// </summary>
    [RelayCommand]
    private async Task Run()
    {
        var result = await Mediator.Send(new NewRun(Node.Id));
        if (Notifier.ShowIfFailed(result)) return;

        var run = new RunObserver(result.Value);

        await Navigator.Navigate(() => new RunDetailPageModel(run, true));
    }

    /// <inheritdoc />
    protected override async Task NavigateTabs()
    {
        if (Node.Type == NodeType.Spec)
        {
            await Navigator.Navigate(() => new CriteriaPageModel(Node));
            await Navigator.Navigate(() => new VariablesPageModel(Node));
            return;
        }

        await Navigator.Navigate(() => new SpecsPageModel(Node));
        await Navigator.Navigate(() => new VariablesPageModel(Node));
    }

    /// <summary>
    /// Loads the full node into the application, replacing the current node instance.
    /// Nofitys changes and then starts tracking for changes.
    /// </summary>
    private async Task LoadNode()
    {
        var result = await Mediator.Send(new LoadNode(Node.Id));
        if (result.IsFailed) return;

        Node = new NodeObserver(result.Value);
        OnPropertyChanged(nameof(Node));
        Track(Node);
    }

    /// <summary>
    /// Loads the local runner page to allow the user to run and test a spec before saving/creating a run.
    /// </summary>
    private async Task LoadRunner()
    {
        if (Node.Type != NodeType.Spec) return;
        RunnerPage = await Navigator.Navigate(() => new SpecRunnerPageModel(Node));
    }

    /// <summary>
    /// Creates this node in the database by prompting the user to select a parent container, and then sending the
    /// CreateNode request with the update node parent id. This should only be used for virtual nodes that have not
    /// yet been saved to the database.
    /// </summary>
    private async Task<Result> CreateNode()
    {
        var parent = await Prompter.Show<NodeObserver?>(() => new SaveToContainerPageModel());

        if (parent is null)
            return Result.Fail("Node requires parent to be saved.");

        parent.Nodes.Add(Node);

        var result = await Mediator.Send(new CreateNode(Node));

        if (result.IsSuccess)
        {
            Messenger.Send(new Observer.Created(Node));
            NotifySaveSuccess();
            AcceptChanges();
            return Result.Ok();
        }

        NotifySaveFailed(result);
        return result;
    }

    /// <summary>
    /// Saves the content of this node to the database. This method is called assuming the node is not virtual
    /// (i.e. has a parent node) so that we can find and update it's content. 
    /// </summary>
    private async Task<Result> SaveNode()
    {
        var result = await Mediator.Send(new SaveNode(Node));

        if (result.IsSuccess)
        {
            NotifySaveSuccess();
            AcceptChanges();
            return Result.Ok();
        }

        NotifySaveFailed(result);
        return result;
    }
}