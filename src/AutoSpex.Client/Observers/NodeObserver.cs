using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentResults;
using JetBrains.Annotations;

namespace AutoSpex.Client.Observers;

[PublicAPI]
public partial class NodeObserver : NamedObserver<Node>,
    IRecipient<Observer<Node>.Created>,
    IRecipient<Observer<Node>.Deleted>
{
    public NodeObserver(Node node) : base(node)
    {
        Nodes = new ObserverCollection<Node, NodeObserver>(
            () => Model.Nodes.Select(n => new NodeObserver(n)).ToList(),
            (_, n) => Model.AddNode(n),
            (_, n) => Model.AddNode(n),
            (_, n) => Model.RemoveNode(n),
            () => Model.ClearNodes());
        
        Track(nameof(Name));
        Track(nameof(Documentation));
    }

    public override Guid Id => Model.NodeId;
    public Guid ParentId => Model.ParentId;
    public NodeType NodeType => Model.NodeType;
    public string Path => Model.Path;
    
    public override string Name
    {
        get => Model.Name;
        set => SetProperty(Model.Name, value, Model, (s, v) => s.Name = v);
    }

    public string Documentation
    {
        get => Model.Documentation;
        set => SetProperty(Model.Documentation, value, Model, (s, v) => s.Documentation = v);
    }

    public ObserverCollection<Node, NodeObserver> Nodes { get; }

    [ObservableProperty] private bool _isVisible = true;

    [ObservableProperty] private bool _isExpanded;

    [ObservableProperty] private bool _isSelected;

    [ObservableProperty] private bool _isEditing;
    
    [ObservableProperty] private bool _isChecked = true;

    [ObservableProperty] private bool _isNew;
    public IEnumerable<Node> CheckedSpecs => CheckedSpecNodes(this);

    #region Commands

    [RelayCommand]
    private async Task AddFolder()
    {
        var node = new NodeObserver(Node.NewFolder()) {IsNew = true};
        Nodes.Add(node);

        var result = await Mediator.Send(new CreateNode(node));

        if (result.IsFailed)
        {
            Nodes.Remove(node);
            return;
        }

        IsExpanded = true;
        IsSelected = false;
        node.IsSelected = true;
        await Navigator.Navigate(node);
    }

    /// <summary>
    /// This command creates a new default spec node and adds it to the current node as a child. It will send the command
    /// to the database to create the new node and upon success we will navigate to this node in the tree and details
    /// by expanding, selecting, and sending the navigation request.
    /// </summary>
    [RelayCommand]
    private async Task AddSpec()
    {
        var node = new NodeObserver(Node.NewSpec()) {IsNew = true};
        Nodes.Add(node);

        var result = await Mediator.Send(new CreateNode(node));

        if (result.IsFailed)
        {
            Nodes.Remove(node);
            return;
        }

        IsExpanded = true;
        IsSelected = false;
        node.IsSelected = true;
        await Navigator.Navigate(node);
    }

    /// <inheritdoc />
    protected override async Task Delete()
    {
        var delete = await Prompter.PromptDelete(Name);
        if (delete is not true) return;

        var result = await Mediator.Send(new DeleteNode(Id));
        if (result.IsFailed) return;

        //We don't have access to the actual parent observer only the underlying model object. Therefore we will rely
        //on messages send to be handles by the parent and remove this node form it's Nodes collection.
        Messenger.Send(new Deleted(this));
    }

    [RelayCommand]
    private void Edit()
    {
        IsEditing = true;
    }

    #endregion

    #region MessageHandlers

    public void Receive(Created message)
    {
        if (message.Observer is not NodeObserver node) return;

        //If I am the parent of the created node then make sure it is added to my Nodes
        if (Id == node.ParentId && Nodes.All(n => n.Id != node.Id))
        {
            // This will also add to the underlying model object which is fine because
            // we assume the node was added to a different model somewhere.
            Nodes.Add(node);
        }
    }

    /// <summary>
    /// Will handle the removal of the node and/or child nodes as based on which observer object is received.
    /// </summary>
    /// <param name="message">The message that an observer has been deleted.</param>
    public void Receive(Deleted message)
    {
        if (message.Observer is not NodeObserver node) return;

        //If the deleted node is my child then remove if from nodes and return.
        //This will in turn remove it from the model object as well.
        if (Nodes.Any(n => n.Id == node.Id))
        {
            Nodes.Remove(node);
            return;
        }

        //If the deleted node is me then send messages for all the child nodes and clear the collection.
        //We need to send messages for children in case they are in use by other pages.
        //Doing in the message handler will all it to be propagated.
        if (Id != node.Id) return;
        Nodes.ToList().ForEach(n => Messenger.Send(new Deleted(n)));
        Messenger.UnregisterAll(this);
    }

    #endregion

    protected override Task<Result> RenameModel(string name) => Mediator.Send(new RenameNode(this));
    public static implicit operator NodeObserver(Node node) => new(node);
    public static implicit operator Node(NodeObserver observer) => observer.Model;
    
    private IEnumerable<Node> CheckedSpecNodes(NodeObserver node)
    {
        var nodes = new List<Node>();

        if (node.NodeType == NodeType.Spec && node.IsChecked)
            nodes.Add(node);
        
        foreach (var child in node.Nodes)
            nodes.AddRange(CheckedSpecNodes(child));

        return nodes;
    }
}