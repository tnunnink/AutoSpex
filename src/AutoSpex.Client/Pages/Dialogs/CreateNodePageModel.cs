using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Pages;

public partial class CreateNodePageModel(NodeType feature) : PageViewModel
{
    private readonly List<NodeObserver> _containers = [];
    
    [ObservableProperty] private NodeType _feature = feature;
    
    [ObservableProperty] private string _title = $"Create new {feature.ToString().ToLowerInvariant()}";

    [ObservableProperty] [NotifyDataErrorInfo] [NotifyCanExecuteChangedFor(nameof(CreateCommand))] [Required]
    private string _name = string.Empty;

    [ObservableProperty] private NodeObserver? _selectedNode;
    public ObservableCollection<NodeObserver> Containers { get; } = [];

    [ObservableProperty] private string _filter = string.Empty;
    

    public override async Task Load()
    {
        var result = await Mediator.Send(new GetContainerNodes(Feature));
        if (result.IsFailed) return;

        var nodes = result.Value.Select(n => new NodeObserver(n));
        _containers.Clear();
        _containers.AddRange(nodes);
        UpdateNodes();
    }

    [RelayCommand(CanExecute = nameof(CanCreate))]
    private async Task Create(Window dialog)
    {
        var node = Node.NewNode(Feature, Name);

        SelectedNode?.Model.AddNode(node);
        
        var result = await Mediator.Send(new CreateNode(node));
        if (result.IsFailed)
        {
            //todo not sure but maybe show failed view.
            return;
        }

        var observer = new NodeObserver(node);
        Messenger.Send(new NodeObserver.Created(observer));
        dialog.Close(observer);
    }

    private bool CanCreate() => !HasErrors && !string.IsNullOrEmpty(Name);

    partial void OnFilterChanged(string value)
    {
        UpdateNodes(value);
    }

    private void UpdateNodes(string? filter = default)
    {
        var filtered = _containers.Where(n => n.Filter(filter));
        Containers.Refresh(filtered);
    }
}