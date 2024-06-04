using System.Collections.Generic;
using AutoSpex.Client.Observers;
using AutoSpex.Engine;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using JetBrains.Annotations;

namespace AutoSpex.Client.Components;

public class ArgumentTemplateSelector : IDataTemplate
{
    [Content] [UsedImplicitly] public Dictionary<string, IDataTemplate> Templates { get; } = new();
    
    public IDataTemplate? ArgumentTemplate { get; set; }

    public Control? Build(object? param)
    {
        if (param is not ArgumentObserver argument) return default;

        return argument.Value is CriterionObserver
            ? Templates[nameof(Criterion)].Build(param)
            : Templates[nameof(Argument)].Build(param);
    }

    public bool Match(object? data) => data is ArgumentObserver;
}