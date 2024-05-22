using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using JetBrains.Annotations;

namespace AutoSpex.Client.Resources;

public class TypeDataTemplates : IDataTemplate
{
    [Content] [UsedImplicitly] public Dictionary<Type, IDataTemplate> Templates { get; } = new();

    public Control? Build(object? param)
    {
        var type = param?.GetType();
        if (type is null) return default;
        return Templates.TryGetValue(type, out var template) ? template.Build(param) : default;
    }

    public bool Match(object? data) => data is not null;
}