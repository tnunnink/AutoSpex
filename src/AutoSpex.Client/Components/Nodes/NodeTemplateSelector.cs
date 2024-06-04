using System.Collections.Generic;
using AutoSpex.Client.Observers;
using AutoSpex.Engine;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using JetBrains.Annotations;

namespace AutoSpex.Client.Components;

public class NodeTemplateSelector : IDataTemplate
{
    [Content] [UsedImplicitly] public Dictionary<NodeType, IDataTemplate> Templates { get; } = new();

    public Control? Build(object? param)
    {
        if (param is not NodeObserver node) return default;
        return Templates.TryGetValue(node.Type, out var template) ? template.Build(node) : default;
    }

    public bool Match(object? data) => data is NodeObserver;
}