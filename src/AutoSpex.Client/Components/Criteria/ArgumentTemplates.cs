using System;
using System.Collections.Generic;
using AutoSpex.Client.Observers;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using JetBrains.Annotations;

namespace AutoSpex.Client.Components;

public class ArgumentTemplates : IDataTemplate
{
    [Content]
    [UsedImplicitly]
    public Dictionary<Type, IDataTemplate> Templates {get;} = new();
    
    public Control? Build(object? param)
    {
        if (param is not ArgumentObserver argument) return default;
        return Templates[argument.Type].Build(param);
    }

    public bool Match(object? data) => data is ArgumentObserver;
}