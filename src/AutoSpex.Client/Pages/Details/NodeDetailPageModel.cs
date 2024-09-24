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
    public NodeDetailPageModel(NodeObserver node) : base(node.Name)
    {
        Node = node;
    }

    public override string Route => $"{Node.Type}/{Node.Id}";
    public override string Icon => Node.Type.Name;
    public NodeObserver Node { get; private set; }

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(RunCommand))]
    private EnvironmentObserver? _environment;

    public Task<IEnumerable<EnvironmentObserver>> Environments => FetchEnvironments();

    /// <inheritdoc />
    public override async Task Load()
    {
        await LoadNode();
        await LoadTargetEnvironment();
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
    /// Runs this node against the target environment. This involves loading the environment and creating a new run
    /// instance with this node (and all descendant spec nodes if a container). Then navigates the run detail page into view
    /// which will execute the created run instance. 
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanRun))]
    private async Task Run()
    {
        if (Environment is null) return;

        //We need to load the full environment to get sources and overrides.
        var loadEnvironment = await Mediator.Send(new LoadEnvironment(Environment.Id));
        if (Notifier.ShowIfFailed(loadEnvironment, $"Failed to load the target environment {Environment.Name}")) return;

        //We also need to get the full node tree loaded to make sure to add descendants (which the local Node does not have).
        var getNode = await Mediator.Send(new GetNode(Node.Id));
        if (Notifier.ShowIfFailed(getNode)) return;

        //Build the run object.
        var run = new RunObserver(new Run(loadEnvironment.Value, getNode.Value));

        //Navigate the run page model to run the container/spec.
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
        return result.IsSuccess ? result.Value.Select(e => new EnvironmentObserver(e)) : [];
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
        Track(Node, false);
    }

    /// <summary>
    /// Get a list of all environments to allow the user to select the one to run against.
    /// </summary>
    private async Task LoadTargetEnvironment()
    {
        var result = await Mediator.Send(new GetTargetEnvironment());
        if (result.IsFailed) return;
        Environment = new EnvironmentObserver(result.Value);
        Track(Environment, false);
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