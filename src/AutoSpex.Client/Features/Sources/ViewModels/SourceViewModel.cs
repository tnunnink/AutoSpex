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
public partial class SourceViewModel : NodeDetailViewModel
{
    private readonly IMediator _mediator;
    
    public SourceViewModel(Node node) : base(node)
    {
    }

    [ObservableProperty] private Source? _source;
    
    [ObservableProperty] private bool _isLoading;
    
    [ObservableProperty] private bool _loadFailed;

    [ObservableProperty] private ObservableCollection<Tag> _tags = new();
    
    public FlatTreeDataGridSource<DataType> DataTypeSource { get; private set; }

    protected override Task Load()
    {
        return base.Load();
    }

    protected override Task Save()
    {
        throw new NotImplementedException();
    }

    protected override bool CanSave()
    {
        throw new NotImplementedException();
    }
}