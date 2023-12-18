using System;
using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Features.Nodes;
using AutoSpex.Client.Features.Sources.Models;
using AutoSpex.Client.Features.Sources.Requests;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using JetBrains.Annotations;
using L5Sharp.Core;
using MediatR;

namespace AutoSpex.Client.Features.Sources;

[UsedImplicitly]
public partial class SourceViewModel : Nodes.NodeDetailViewModel
{
    private readonly IMediator _mediator;
    
    public SourceViewModel(Node node) : base(node, new SourceView())
    {
        _mediator = App.Container.GetInstance<IMediator>();
        Run = LoadSource(node.NodeId);
    }

    [ObservableProperty] private Source? _source;
    
    [ObservableProperty] private bool _isLoading;
    
    [ObservableProperty] private bool _loadFailed;

    [ObservableProperty] private ObservableCollection<Tag> _tags = new();
    
    public FlatTreeDataGridSource<DataType> DataTypeSource { get; private set; }
    
    protected override Task Save()
    {
        throw new NotImplementedException();
    }

    private async Task LoadSource(Guid nodeId)
    {
        var result = await _mediator.Send(new GetSourceRequest(nodeId));
        if (result.IsFailed)
        {
            LoadFailed = true;
        }

        Source = result.Value;
        
        var data = Source.Content.Tags.Take(20).ToList();
        Tags.AddRange(data);
        
        /*DataTypeSource = new FlatTreeDataGridSource<DataType>(_dataTypes)
        {
            Columns =
            {
                new TextColumn<DataType, string>("Name", x => x.Name),
                new TextColumn<DataType, string>("Description", x => x.Description),
                new TextColumn<DataType, string>("Container", x => x.Container),
                new TextColumn<DataType, int>("Members", x => x.Members.Count()),
            }
        };*/
    }
}