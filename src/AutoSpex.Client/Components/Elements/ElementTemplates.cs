using System;
using System.Collections.Generic;
using System.Linq;
using AutoSpex.Client.Observers;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using JetBrains.Annotations;

namespace AutoSpex.Client.Components;

/*public class ElementTemplates : IDataTemplate
{
    [Content] [UsedImplicitly] public Dictionary<Type, IDataTemplate> Templates { get; } = new();

    public Control? Build(object? param)
    {
        if (param is not ElementObserver observer) return default;
        return Templates.FirstOrDefault(t => observer.Element.Type.IsAssignableTo(t.Key)).Value.Build(param);
    }

    public bool Match(object? data) => data is ElementObserver;
}*/