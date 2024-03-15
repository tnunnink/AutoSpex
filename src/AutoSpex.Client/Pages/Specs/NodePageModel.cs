using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Pages;

public partial class NodePageModel : DetailPageModel,
    IRecipient<NodeObserver.Renamed>,
    IRecipient<NodeObserver.Deleted>
{
    /// <summary>
    /// This is the node that is passed in to this details page from a navigation tree. It does not contain.
    /// </summary>
    private readonly NodeObserver _target;

    /// <inheritdoc/>
    public NodePageModel(NodeObserver target)
    {
        _target = target;
        Breadcrumb = new Breadcrumb(_target, CrumbType.Target);
    }

    public override string Route => $"Node/{_target.Id}";
    public override string Title => _target.Name;
    public override string Icon => _target.NodeType.Name;
    public Breadcrumb Breadcrumb { get; }

    /// <summary>
    /// The fully loaded <see cref="NodeObserver"/> which will contain all the detail data for this node. This would
    /// include variables for the node and the spec configuration if this node is a spec type node.
    /// </summary>
    [ObservableProperty] private NodeObserver? _node;
    
    public Task<ObservableCollection<SourceObserver>> Sources => RequestSources();

    private async Task<ObservableCollection<SourceObserver>> RequestSources()
    {
        var result = await Mediator.Send(new ListSources());
        if (result.IsFailed) return [];
        var sources = result.Value.Select(s => new SourceObserver(s));
        return new ObservableCollection<SourceObserver>(sources);
    }

    /// <inheritdoc />
    public override async Task Load()
    {
        var result = await Mediator.Send(new GetFullNode(_target.Id));

        if (result.IsFailed || result.Value is null)
        {
            //todo navigate node not found...
            return;
        }

        Node = result.Value;
        Track(Node);
        
        await NavigateTabs(Node);
    }

    protected override async Task Save()
    {
        if (Node is null) return;

        var result = await Mediator.Send(new SaveNode(Node));
        if (result.IsFailed) return;

        NotifySaveSuccess(Node);
        AcceptChanges();
    }

    protected override bool CanSave() => base.CanSave() && Node is not null;

    private async Task Run(SourceObserver? source)
    {
        if (source is null || Node is null) return;
        var runner = new Runner();
        runner.AddNode(Node);
        var observer = new RunnerObserver(runner);
        await Navigator.Navigate(observer);
    }
    
    [RelayCommand]
    private async Task Run(RunnerObserver? runner)
    {
        if (runner is null) return;
        await Navigator.Navigate(runner);
    }

    public override void Receive(NavigationRequest message)
    {
        if (!message.Page.Route.Contains(Route)) return;

        switch (message.Page)
        {
            case FiltersPageModel or VerificationsPageModel or VariablesPageModel or SpecOptionsPageModel:
                NavigateTabPage(message.Page, message.Action);
                break;
            case NodeInfoPageModel:
                NavigateDetailPage(message.Page, message.Action);
                break;
        }
    }

    public void Receive(Observer<Node>.Deleted message)
    {
        if (message.Observer.Id != _target.Id) return;
        ForceClose();
    }

    public void Receive(Observer<Node>.Renamed message)
    {
        if (message.Observer is not NodeObserver node) return;
        if (node.Id != _target.Id) return;
        OnPropertyChanged(nameof(Title));
    }

    public override bool Equals(object? obj) => obj is NodePageModel other && _target.Id == other._target.Id;
    public override int GetHashCode() => _target.Id.GetHashCode();

    private void NotifySaveSuccess(NodeObserver node)
    {
        var title = $"{node.NodeType} Saved";
        var message = $"{node.Name} was saved successfully @ {DateTime.Now.ToShortTimeString()}";
        var notification = new Notification(title, message, NotificationType.Success);
        Notifier.Notify(notification);
    }

    private async Task NavigateTabs(NodeObserver node)
    {
        if (node.NodeType == NodeType.Collection)
        {
            await Navigator.Navigate(() => new VariablesPageModel(node));
            await Navigator.Navigate(() => new NodeInfoPageModel(node));
        }

        if (node.NodeType == NodeType.Folder)
        {
            await Navigator.Navigate(() => new VariablesPageModel(node));
            await Navigator.Navigate(() => new NodeInfoPageModel(node));
        }

        if (node.NodeType == NodeType.Spec)
        {
            await Navigator.Navigate(() => new FiltersPageModel(node));
            await Navigator.Navigate(() => new VerificationsPageModel(node));
            await Navigator.Navigate(() => new VariablesPageModel(node));
            await Navigator.Navigate(() => new SpecOptionsPageModel(node));
            await Navigator.Navigate(() => new NodeInfoPageModel(node));
        }
    }
}