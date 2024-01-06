using System;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using JetBrains.Annotations;
using L5Sharp.Core;
using MediatR;
using NodeObserver = AutoSpex.Client.Observers.NodeObserver;
using SourceObserver = AutoSpex.Client.Observers.SourceObserver;

namespace AutoSpex.Client.ViewModels;

[UsedImplicitly]
public partial class SourceViewModel : NodeDetailViewModel
{
    private readonly IMediator _mediator;
    
    public SourceViewModel(NodeObserver node) : base(node)
    {
    }

    [ObservableProperty] private SourceObserver? _source;
    
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