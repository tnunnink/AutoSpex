using System.Collections.Generic;
using AutoSpex.Engine;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;

namespace AutoSpex.Client.Resources.DataTemplates;

public class ArgumentTemplates : IDataTemplate
{
    [Content]
    public Dictionary<TypeGroup, IDataTemplate> Templates {get;} = new();
    
    public Control? Build(object? param)
    {
        if (param is not Argument arg) return default;
        return Templates[arg.Group].Build(arg);
    }

    public bool Match(object? data)
    {
        return data is Argument;
    }
}