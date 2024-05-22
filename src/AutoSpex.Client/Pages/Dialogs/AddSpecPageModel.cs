using System;
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
using DynamicData;

namespace AutoSpex.Client.Pages;

public partial class AddSpecPageModel : PageViewModel
{
    private readonly List<NodeObserver> _selectableNodes = [];

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotifyCanExecuteChangedFor(nameof(CreateCommand))]
    [Required]
    [MaxLength(100)]
    private string _name = string.Empty;

    [ObservableProperty] private bool _showDescription;

    [ObservableProperty] private string _documentation = string.Empty;
    public ObservableCollection<NodeObserver> Nodes { get; } = [];

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(CreateCommand))]
    private NodeObserver? _selectedNode;

    [ObservableProperty] private string _searchText = string.Empty;

    [ObservableProperty] private string _addToolTip = "Add Collection";

    public override async Task Load()
    {
        var result = await Mediator.Send(new GetContainerNodes(NodeType.Spec));
        if (result.IsFailed) return;

        var nodes = result.Value.Select(n => new NodeObserver(n));
        _selectableNodes.Clear();
        _selectableNodes.AddRange(nodes);
        UpdateNodes();
    }

    [RelayCommand(CanExecute = nameof(CanCreate))]
    private async Task Create(Window dialog)
    {
        if (SelectedNode is null) return;

        var spec = SelectedNode.Model.AddSpec(Name);
        var result = await Mediator.Send(new CreateNode(spec));
        if (result.IsFailed)
        {
            //todo not sure but maybe show failed view.
            return;
        }

        var observer = new NodeObserver(spec);
        Messenger.Send(new NodeObserver.Created(observer));
        dialog.Close(observer);
    }

    private bool CanCreate() => !HasErrors && !string.IsNullOrEmpty(Name) && SelectedNode is not null;

    [RelayCommand]
    private void AddDescription()
    {
        ShowDescription = true;
    }

    partial void OnSearchTextChanged(string value)
    {
        UpdateNodes(value);
    }

    private void UpdateNodes(string? filter = default)
    {
        Nodes.Clear();

        if (string.IsNullOrEmpty(filter))
        {
            Nodes.AddRange(_selectableNodes);
            return;
        }

        var filtered = _selectableNodes.Where(n =>
            n.Name.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
            n.Path.Contains(filter, StringComparison.OrdinalIgnoreCase));

        Nodes.AddRange(filtered);
    }
}