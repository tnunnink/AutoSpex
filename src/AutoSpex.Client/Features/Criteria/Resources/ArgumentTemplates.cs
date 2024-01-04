using System;
using System.Collections.Generic;
using AutoSpex.Engine;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;

namespace AutoSpex.Client.Features.Criteria;

public class ArgumentTemplates : IDataTemplate
{
    [Content]
    public Dictionary<TypeGroup, IDataTemplate> Templates {get;} = new();
    
    public Control? Build(object? param)
    {
        if (param is not Arg arg) return default;
        return Templates[arg.Property.Group].Build(arg);
    }

    public bool Match(object? data)
    {
        return data is Arg;
    }
}