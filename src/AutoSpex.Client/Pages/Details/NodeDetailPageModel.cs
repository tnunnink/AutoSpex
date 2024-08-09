using System;
using System.Collections.Generic;
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

public partial class NodeDetailPageModel : DetailPageModel, IRecipient<EnvironmentObserver.Targeted>
{
    /// <inheritdoc/>
    public NodeDetailPageModel(NodeObserver node)
    {
        Node = node;
    }

    public override string Route => $"{Node.Type}/{Node.Id}";
    public override string Title => Node.Name;
    public override string Icon => Node.Type.Name;
    public NodeObserver Node { get; }

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(RunCommand))]
    private EnvironmentObserver? _environment;

    public Task<IEnumerable<EnvironmentObserver>> Environments => FetchEnvironments();

    /// <inheritdoc />
    public override async Task Load()
    {
        await base.Load();
        await LoadTargetEnvironment();
        SaveCommand.NotifyCanExecuteChanged();
    }

    /// <inheritdoc />
    public override async Task<Result> Save()
    {
        //If this node is a collection or has an assigned parent already, we can continue saving.
        if (Node.Type == NodeType.Collection || Node.ParentId != Guid.Empty) return await base.Save();

        //If this node is virtual with no parent we need to select a parent and create then node before saving anything.
        var parent = await Prompter.Show<NodeObserver?>(() => new SaveToContainerPageModel());
        if (parent is null) return Result.Fail("Node requires parent to be saved.");

        parent.Nodes.Add(Node);
        var result = await Mediator.Send(new CreateNode(Node));
        if (result.IsSuccess)
        {
            Messenger.Send(new Observer.Created(Node));
            return await base.Save();
        }

        NotifySaveFailed(result);
        return result;
    }

    /// <inheritdoc />
    /// <remarks>If this node is virtual then we need to enable the save command.</remarks>
    public override bool CanSave()
    {
        return base.CanSave() || (Node.Type != NodeType.Collection && Node.ParentId == Guid.Empty);
    }

    /// <summary>
    /// Runs this node against the target environment. This involves loading the environment and creating a new run
    /// instance with this node (and all descendant nodes if a container). Then navigates the run detail page into view
    /// which will execute the created run instance. 
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanRun))]
    private async Task Run()
    {
        if (Environment is null) return;

        //We need to load the full environment to get sources and overrides.
        var result = await Mediator.Send(new LoadEnvironment(Environment.Id));
        if (result.IsFailed) return;
        var environment = new EnvironmentObserver(result.Value);

        //Create new run instance with the target environment and current node.
        var run = new Run(environment, Node);

        await Navigator.Navigate(() => new RunDetailPageModel(run));
    }

    /// <summary>
    /// Determines if the run command can be executed for this node page model.
    /// </summary>
    private bool CanRun() => Environment is not null;

    /// <summary>
    /// When the targeted environment changes from any node page or anywhere else, update the local selected environment
    /// instance.
    /// </summary>
    public void Receive(EnvironmentObserver.Targeted message)
    {
        Environment = message.Environment;
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
    /// Get a list of all environments to allow the user to select the one to run against.
    /// </summary>
    private async Task<IEnumerable<EnvironmentObserver>> FetchEnvironments()
    {
        var result = await Mediator.Send(new ListEnvironments());

        return result.IsSuccess
            ? result.Value.Select(e => new EnvironmentObserver(e))
            : Enumerable.Empty<EnvironmentObserver>();
    }

    /// <summary>
    /// Get a list of all environments to allow the user to select the one to run against.
    /// </summary>
    private async Task LoadTargetEnvironment()
    {
        var result = await Mediator.Send(new GetTargetEnvironment());
        if (result.IsFailed) return;
        Environment = new EnvironmentObserver(result.Value);
    }
}