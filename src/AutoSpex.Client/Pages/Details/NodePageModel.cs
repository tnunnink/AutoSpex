using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoSpex.Client.Messages;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentResults;

namespace AutoSpex.Client.Pages;

public partial class NodePageModel : DetailPageModel,
    IRecipient<NavigationRequest>,
    IRecipient<NodeObserver.Renamed>,
    IRecipient<NodeObserver.Deleted>,
    IRecipient<VariableObserver.Deleted>
{
    /// <inheritdoc/>
    public NodePageModel(NodeObserver node)
    {
        Node = node;
        Breadcrumb = new Breadcrumb(Node, CrumbType.Target);
    }

    public override string Route => $"Node/{Node.Id}";
    public override string Title => Node.Name;
    public override string Icon => Node.NodeType.Name;
    public NodeObserver Node { get; }
    public Breadcrumb Breadcrumb { get; }
    public ObserverCollection<Variable, VariableObserver> Variables { get; } = [];
    [ObservableProperty] private SpecObserver? _spec;
    public ObservableCollection<SourceObserver> Sources => new(RequestSources());

    public ObservableCollection<PageViewModel> Tabs { get; } = [];
    [ObservableProperty] private PageViewModel? _selectedTab;

    public override async Task Load()
    {
        await LoadVariables(Node.Id);
        await LoadSpec();
        SaveCommand.NotifyCanExecuteChanged();

        await NavigateTabs();
    }

    private async Task NavigateTabs()
    {
        if (Node.NodeType != NodeType.Spec)
            await Navigator.Navigate(() => new SpecListPageModel(Node));

        if (Node.NodeType == NodeType.Spec && Spec is not null)
            await Navigator.Navigate(() => new SpecCriteriaPageModel(Spec));

        await Navigator.Navigate(() => new NodeVariablesPageModel(Node));

        if (Node.NodeType == NodeType.Spec && Spec is not null)
            await Navigator.Navigate(() => new SpecSettingsPageModel(Spec));

        await Navigator.Navigate(() => new NodeRunsPageModel(Node));
        await Navigator.Navigate(() => new NodeInfoPageModel(Node));
    }

    protected override async Task<Result> Save()
    {
        var nodeResult = await Mediator.Send(new UpdateNode(Node));
        var variablesResult = await Mediator.Send(new SaveVariables(Variables.Select(v => v.Model)));
        var specResult = Spec is not null ? await Mediator.Send(new SaveSpec(Spec)) : Result.Ok();

        var result = Result.Merge(nodeResult, variablesResult, specResult);
        if (result.IsFailed) return result;

        NotifySaveSuccess();
        AcceptChanges();
        return result;
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
    private async Task Run(SourceObserver? source)
    {
        //If a source is not provided then use the selected. 
        source ??= Sources.SingleOrDefault(s => s.IsSelected);

        //Load all spec configs with variables resolved. 
        var specs = await LoadSpecs(CancellationToken.None);

        //Create the run object
        var run = new Run(source?.Model);
        run.AddSpecs(specs);

        //todo we will set the RunOnLoad here eventually.
        await Navigator.Navigate(new RunObserver(run));
    }

    private bool CanRun() => Sources.Any();
    
    /// <summary>
    /// 
    /// </summary>
    public void Receive(NavigationRequest message)
    {
        if (!message.Page.Route.StartsWith(Route)) return;
        
        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (message.Action)
        {
            case NavigationAction.Open:
                Tabs.Add(message.Page);
                break;
            case NavigationAction.Close:
                Tabs.Remove(message.Page);
                break;
        }
    }

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

    public void Receive(Observer<Variable>.Deleted message)
    {
        if (message.Observer is not VariableObserver variable || !Variables.Contains(variable)) return;
        Variables.Remove(variable);
    }

    /// <summary>
    /// Sends a notification to the UI that the node was saved successfully.
    /// </summary>
    private void NotifySaveSuccess()
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
    /// Loads the spec configuration for this node if this is a spec node and not a collection or folder.
    /// </summary>
    private async Task LoadSpec()
    {
        if (Node.NodeType != NodeType.Spec) return;

        var result = await Mediator.Send(new GetSpec(Node.Id));
        if (result.IsFailed) return;

        Spec = new SpecObserver(result.Value);
        Track(Spec);
    }

    /// <summary>
    /// Load all specification configurations that are checked to run for this node page.
    /// Also, resolveS all required variables for the specs.
    /// This will ensure we have all the data necessary to perform the run and update the user interface.
    /// </summary>
    private async Task<IEnumerable<Spec>> LoadSpecs(CancellationToken token)
    {
        var ids = Node.CheckedSpecs.Select(s => s.NodeId);

        var load = await Mediator.Send(new GetSpecsIn(ids), token);
        if (load.IsFailed) return Enumerable.Empty<Spec>();

        await Mediator.Send(new ResolveVariables(load.Value), token);
        return load.Value;
    }

    /// <summary>
    /// Get all collection sources for display in the run button. Since all sources are loaded in the navigation tree
    /// we can send this request without having get it again from the database. 
    /// </summary>
    private IEnumerable<SourceObserver> RequestSources()
    {
        var request = new SourceRequest();
        Messenger.Send(request);
        return request.HasReceivedResponse ? request.Response : Enumerable.Empty<SourceObserver>();
    }
}