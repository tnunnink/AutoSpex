using System;
using AutoSpex.Client.Shared;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace AutoSpex.Client.Services;

public class ViewLocator : IDataTemplate
{
    public Control Build(object? data)
    {
        if (data is null)
            throw new ArgumentNullException(nameof(data), "ViewLocator requires a non null model object.");
        
        if (data is not ViewModelBase)
            throw new InvalidOperationException($"ViewLocator can not resolve the type {data.GetType()}");

        var name = data.GetType().FullName!.Replace("ViewModel", "View");
        var type = Type.GetType(name);

        return type is not null
            ? (Control) Activator.CreateInstance(type)!
            : new TextBlock {Text = "Not Found: " + name};
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}