using System;
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
    public NodeObserver Node { get; }
    
    [ObservableProperty] private bool _showDrawer;

    [ObservableProperty] private ResultDrawerPageModel? _resultDrawer;

    /// <inheritdoc />
    public override async Task Load()
    {
        await base.Load();
        
        ResultDrawer = new ResultDrawerPageModel();
        RegisterDisposable(ResultDrawer);

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
        var errors = Validate().ToList();

        if (errors.Count > 0)
        {
            Notifier.ShowError($"Failed to save {Title}", $"{errors.FirstOrDefault()?.ErrorMessage}");
            return Result.Fail("Failed to save page due to validation errors.");
        }

        result = Node.IsVirtual ? await CreateNode() : Result.Ok();

        return await base.Save(result);
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
        await Node.RunCommand.ExecuteAsync(null);
    }

    /// <inheritdoc />
    protected override async Task NavigatePages()
    {
        if (Node.Type != NodeType.Spec)
        {
            await Navigator.Navigate(() => new SpecsPageModel(Node));
        }
        else
        {
            await Navigator.Navigate(() => new SpecPageModel(Node));
        }

        /*await Navigator.Navigate(() => new VariablesPageModel(Node));*/
        await Navigator.Navigate(() => new InfoPageModel(Node));
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