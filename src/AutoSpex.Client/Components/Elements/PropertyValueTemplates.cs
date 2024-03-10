using System.Collections.Generic;
using AutoSpex.Client.Observers;
using AutoSpex.Engine;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using JetBrains.Annotations;

namespace AutoSpex.Client.Components;

public class PropertyValueTemplates : IDataTemplate
{
    [Content] [UsedImplicitly] public Dictionary<TypeGroup, IDataTemplate> Templates { get; } = new();

    public Control? Build(object? param)
    {
        if (param is not PropertyObserver observer) return default;
        return Templates.TryGetValue(observer.Model.Group, out var template) ? template.Build(param) : default;
    }

    public bool Match(object? data) => data is PropertyObserver;
}