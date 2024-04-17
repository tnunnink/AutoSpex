using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AutoSpex.Client.Messages;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using AutoSpex.Persistence.Variables;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentResults;

namespace AutoSpex.Client.Pages;

public abstract partial class NodePageModel : DetailPageModel,
    IRecipient<NodeObserver.Renamed>,
    IRecipient<NodeObserver.Deleted>
{
    /// <inheritdoc/>
    protected NodePageModel(Node node)
    {
        Node = node;
        Breadcrumb = new Breadcrumb(Node, CrumbType.Target);
    }

    public override string Route => $"Node/{Node.Id}";
    public override string Title => Node.Name;
    public override string Icon => Node.NodeType.Name;
    protected NodeObserver Node { get; }
    public Breadcrumb Breadcrumb { get; }
    public ObserverCollection<Variable, VariableObserver> Variables { get; } = [];
    public ObservableCollection<SourceObserver> Sources => new(RequestSources());

    public override async Task Load()
    {
        await LoadVariables(Node.Id);
    }

    protected override async Task<Result> Save()
    {
        var nodeResult = await Mediator.Send(new UpdateNode(Node));
        var variablesResult = await Mediator.Send(new SaveVariables(Variables.Select(v => v.Model)));
        return Result.Merge(nodeResult, variablesResult);
    }

    [RelayCommand]
    private void AddVariable()
    {
        var variable = new Variable(Node.Id);
        Variables.Add(new VariableObserver(variable));
    }

    [RelayCommand]
    private void RemoveVariable(VariableObserver? variable)
    {
        if (variable is null) return;
        Variables.Remove(variable);
    }

    [RelayCommand(CanExecute = nameof(CanRun))]
    protected virtual Task Run(SourceObserver? source) => Task.CompletedTask;

    protected bool CanRun() => Sources.Any(s => s.IsSelected);

    /// <summary>
    /// Close this detail page if the node was deleted.
    /// </summary>
    public void Receive(Observer<Node>.Deleted message)
    {
        if (message.Observer.Id != Node.Id) return;
        ForceClose();
    }

    /// <summary>
    /// Notify the <see cref="Title"/> property change when a node is renamed.
    /// </summary>
    public void Receive(NamedObserver<Node>.Renamed message)
    {
        if (message.Observer.Id != Node.Id) return;
        OnPropertyChanged(nameof(Title));
    }
    
    /// <summary>
    /// Sends a notification to the UI that the node was saved successfully.
    /// </summary>
    protected void NotifySaveSuccess()
    {
        var title = $"{Node.NodeType} Saved";
        var message = $"{Node.Name} was saved successfully @ {DateTime.Now.ToShortTimeString()}";
        var notification = new Notification(title, message, NotificationType.Success);
        Notifier.Notify(notification);
    }

    /// <summary>
    /// Load the variables defined for this node and initialize the variable collection with that data.
    /// </summary>
    private async Task LoadVariables(Guid nodeId)
    {
        var result = await Mediator.Send(new GetNodeVariables(nodeId));
        if (result.IsFailed) return;
        Variables.Refresh(result.Value.Select(v => new VariableObserver(v)));
        Track(Variables);
    }
    
    /// <summary>
    /// Get all collection sources for display in the run button. Since all sources are loaded in the navigation tree
    /// we can send this request without having get it again from the database. 
    /// </summary>
    private IEnumerable<SourceObserver> RequestSources()
    {
        var request = new SourceRequest();
        Messenger.Send(request);
        if (!request.HasReceivedResponse) return Enumerable.Empty<SourceObserver>();
        return request.Response;
    }
}