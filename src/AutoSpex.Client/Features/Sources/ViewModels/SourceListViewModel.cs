using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AutoSpex.Client.Features.Nodes;
using AutoSpex.Client.Features.Sources.Requests;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using DynamicData.Binding;
using HanumanInstitute.MvvmDialogs;
using JetBrains.Annotations;
using MediatR;
using Node = AutoSpex.Client.Features.Nodes.Node;

namespace AutoSpex.Client.Features.Sources;

[UsedImplicitly]
public partial class SourceListViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly IMessenger _messenger;
    private readonly IDialogService _dialog;
    
    
    private readonly List<Node> _allNodes = new();
    private readonly SourceCache<Node, Guid> _cache = new(x => x.NodeId);
    private readonly ReadOnlyObservableCollection<Node> _nodes;
    
    public SourceListViewModel(IMediator mediator, IMessenger messenger, IDialogService dialog)
    {
        _mediator = mediator;
        _messenger = messenger;
        _dialog = dialog;

        _cache.Connect()
            .Sort(SortExpressionComparer<Node>.Ascending(t => t.Ordinal))
            .Filter(x => x.Name.ToString().Contains(Filter))
            .Bind(out _nodes)
            .Subscribe();
    }
    
    public ReadOnlyObservableCollection<Node> Nodes => _nodes;

    [ObservableProperty] private Node? _selected;

    [ObservableProperty] private string _filter = string.Empty;

    [RelayCommand]
    private async Task LoadSources()
    {
        var result = await _mediator.Send(new GetNodesRequest(Feature.Sources));

        if (result.IsSuccess)
        {
            _allNodes.Clear();
            _allNodes.AddRange(result.Value);
            _cache.AddOrUpdate(_allNodes.ToArray());
        }
    }
    
    [RelayCommand]
    private async Task AddSource()
    {
        /*var info = await _dialog.Show<dynamic?>(new AddSourceView(), "Add Source");
        if (info is null) return;
        
        var result = await _mediator.Send(new AddSourceRequest(info.Path, info.Name));

        if (result.IsSuccess)
        {
            _allNodes.Add(result.Value);
            _cache.AddOrUpdate(_allNodes.ToArray());
            //todo send message to open the source into the details page.
        }*/
        throw new NotImplementedException();
    }

    [RelayCommand]
    private Task DeleteSource(Node node)
    {
        throw new NotImplementedException();
    }
    
    [RelayCommand]
    private Task MoveSource(Node node)
    {
        throw new NotImplementedException();
    }

    partial void OnSelectedChanged(Node? value)
    {
        if (value is not null)
        {
            _messenger.Send(new OpenNodeMessage(value));
        }
    }
}