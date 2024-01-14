using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;

namespace AutoSpex.Client.Features;

public class BreadcrumbTemplates : IDataTemplate
{
    [Content]
    public Dictionary<CrumbType, IDataTemplate> Templates {get;} = new();

    public Control? Build(object? data)
    {
        if (data is not Breadcrumb crumb) return default;
        return Templates[crumb.Type].Build(crumb);
    }

    public bool Match(object? data) => data is Breadcrumb;
}