using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JetBrains.Annotations;
using L5Spex.Client.Common;
using L5Spex.Client.Notifications;
using L5Spex.Client.Observers;
using L5Spex.Client.Requests.Nodes;
using MediatR;

namespace L5Spex.Client.ViewModels;

[UsedImplicitly]
public partial class SpecTreeViewModel : ViewModelBase
{
    private readonly IMediator _mediator;

    private string _filterText = string.Empty;

    public SpecTreeViewModel()
    {
        _mediator = null!;
    }

    public SpecTreeViewModel(IMediator mediator)
    {
        _mediator = mediator;
        Run = Initialize();
    }

    [ObservableProperty] private ObservableCollection<NodeObserver> _projects = new();

    [ObservableProperty] private ObservableCollection<NodeObserver> _selectedNodes = new();

    [ObservableProperty] private NodeObserver? _selectedNode;

    public string FilterText
    {
        get => _filterText;
        set
        {
            SetProperty(ref _filterText, value);
            FilterOnText(value);
        }
    }

    private void FilterOnText(string text)
    {
        foreach (var node in Projects)
        {
            node.FilterPath(text);
        }
    }

    [RelayCommand]
    private async Task AddProject()
    {
        var request = new AddProjectRequest("New Project");
        var result = await _mediator.Send(request);

        result.IfSucc(n =>
        {
            Projects.Add(n);
            n.IsSelected = true;
            var message = new TestRequest.Message($"Created Project {n.Name}", "Looks like it worked");
            _mediator.Publish(message);
        });

        result.IfFail(e =>
        {
            var message = new TestRequest.Message("Failed to Create Project", e.Message);
            _mediator.Publish(message);
        });
    }

    [RelayCommand]
    private async Task AddFolder(NodeObserver parent)
    {
        var request = new AddNodeRequest(parent.NodeId, NodeType.Folder, "New Folder");
        var result = await _mediator.Send(request);

        result.IfSucc(n =>
        {
            parent.Nodes.Add(n);
            parent.IsExpanded = true;
            parent.IsSelected = false;
            n.IsSelected = true;
            var message = new TestRequest.Message($"Created Project {n.Name}", "Looks like it worked");
            _mediator.Publish(message);
        });

        result.IfFail(e =>
        {
            var message = new TestRequest.Message("Failed to Create Project", e.Message);
            _mediator.Publish(message);
        });
    }

    [RelayCommand]
    private async Task AddSpecification(NodeObserver parent)
    {
        var request = new AddNodeRequest(parent.NodeId, NodeType.Specification, "New Specification");
        var result = await _mediator.Send(request);

        result.IfSucc(n =>
        {
            parent.Nodes.Add(n);
            parent.IsExpanded = true;
            parent.IsSelected = false;
            n.IsSelected = true;
            var message = new TestRequest.Message($"Created Project {n.Name}", "Looks like it worked");
            _mediator.Publish(message);
        });

        result.IfFail(e =>
        {
            var message = new TestRequest.Message("Failed to Create Project", e.Message);
            _mediator.Publish(message);
        });
    }

    [RelayCommand]
    private static void RenameNode(NodeObserver? node)
    {
        if (node is null) return;
        node.IsEditing = true;
    }

    [RelayCommand]
    private async Task DeleteNode(NodeObserver? node)
    {
        if (node is null) return;
        
        //todo we need to issue a confirmation dialog first.
        
        var request = new DeleteNodeRequest(node.NodeId);
        var result = await _mediator.Send(request);

        var notification = result.Match<Notification>(
            _ =>
            {
                Projects.Remove(node);
                return new Notification($"{node.Name} Deleted.",
                    $"The {node.NodeType} project has been successfully deleted.", NotificationType.Success);
            },
            e => new Notification($"Failed to delete {node.Name}.",
                $"The {node.NodeType} project failed to delete. {e.Message}", NotificationType.Error));
        
        /*await _mediator.Publish(notification);*/
    }

    public void Rename(NodeObserver node)
    {
        Run = Task.Run(async () =>
        {
            var request = new RenameNodeRequest(node.NodeId, node.Name);
            var result = await _mediator.Send(request);
            //todo probably send notification or something so others can update or refresh bindings
        });
    }

    private async Task Initialize()
    {
        var request = new GetNodeTreeRequest();
        var result = await _mediator.Send(request);

        result.IfSucc(projects =>
        {
            foreach (var project in projects)
            {
                Projects.Add(project);
            }
        });
    }
}