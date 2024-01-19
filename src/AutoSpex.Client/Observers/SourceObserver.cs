using System;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoSpex.Client.Components.Sources;

public partial class SourceObserver : ObservableObject
{
    private readonly Source _source;

    public SourceObserver(Source source)
    {
        _source = source;
    }

    [ObservableProperty] private Guid _nodeId;

    [ObservableProperty] private string _controller;

    [ObservableProperty] private string? _processor;

    [ObservableProperty] private string? _revision;

    [ObservableProperty] private bool _isContext;

    [ObservableProperty] private string? _targetType;

    [ObservableProperty] private string? _targetName;

    [ObservableProperty] private string? _exportedBy;

    [ObservableProperty] private DateTime? _exportedOn;
}