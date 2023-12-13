using System;
using CommunityToolkit.Mvvm.ComponentModel;
using L5Sharp.Core;

namespace AutoSpex.Client.Features.Sources.Models;

public partial class Source : ObservableObject
{
    public Source(dynamic record)
    {
        NodeId = record.NodeId;
        Controller = record.Controller;
        Processor = record.Processor;
        Revision = record.Revision;
        IsContext = int.Parse(record.IsContext) == 1;
        TargetType = record.TargetType;
        TargetName = record.TargetName;
        ExportedBy = record.ExportedBy;
        ExportedOn = record.ExportedOn;
        Content = L5X.Parse(record.Content.ToString());
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

    public L5X Content { get; }
}