using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Messages;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using DynamicData.Binding;
using HanumanInstitute.MvvmDialogs;
using JetBrains.Annotations;
using MediatR;
using NodeObserver = AutoSpex.Client.Observers.NodeObserver;

namespace AutoSpex.Client.ViewModels;

[UsedImplicitly]
public partial class SourceListViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly IMessenger _messenger;
    private readonly IDialogService _dialog;
    
    
    private readonly List<NodeObserver> _allNodes = new();
    private readonly SourceCache<NodeObserver, Guid> _cache = new(x => x.Model.NodeId);
    private readonly ReadOnlyObservableCollection<NodeObserver> _nodes;
    
    public SourceListViewModel(IMediator mediator, IMessenger messenger, IDialogService dialog)
    {
        _mediator = mediator;
        _messenger = messenger;
        _dialog = dialog;

        _cache.Connect()
            .Sort(SortExpressionComparer<NodeObserver>.Ascending(t => t.Ordinal))
            .Filter(x => x.Name.ToString().Contains((string) Filter))
            .Bind(out _nodes)
            .Subscribe();
    }
    
    public ReadOnlyObservableCollection<NodeObserver> Nodes => _nodes;

    [ObservableProperty] private NodeObserver? _selected;

    [ObservableProperty] private string _filter = string.Empty;

    [RelayCommand]
    private async Task LoadSources()
    {
        var result = await _mediator.Send(new GetNodes(Feature.Sources));

        if (result.IsSuccess)
        {
            _allNodes.Clear();
            _allNodes.AddRange(result.Value.Select(n => (NodeObserver)n));
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
    private Task DeleteSource(NodeObserver node)
    {
        throw new NotImplementedException();
    }
    
    [RelayCommand]
    private Task MoveSource(NodeObserver node)
    {
        throw new NotImplementedException();
    }

    partial void OnSelectedChanged(NodeObserver? value)
    {
        if (value is not null)
        {
            _messenger.Send(new OpenNodeMessage(value));
        }
    }
}