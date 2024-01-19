using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace AutoSpex.Client.Shared;

/// <summary>
/// Maps our page modes to their corresponding pages.
/// </summary>
public class PageLocator : IDataTemplate
{
    /// <inheritdoc />
    public Control Build(object? data)
    {
        if (data is not PageViewModel model)
            throw new InvalidOperationException(
                $"Unexpected type received at PageLocator: {data?.GetType().FullName}");
        
        var pageName = GetPageName(model);
        var pageType = Type.GetType(pageName);

        if (pageType is not null)
        {
            return Activator.CreateInstance(pageType) as Control ?? new TextBlock { Text = "Page Not Found: " + pageName }; 
        }

        return new TextBlock { Text = "Page Not Found: " + pageName };
    }

    /// <inheritdoc />
    public bool Match(object? data) => data is PageViewModel;
    
    private static string GetPageName(PageViewModel model)
    {
        var typeName = model.GetType().FullName;

        if (typeName is null)
            throw new ArgumentException("Can not create page from null type name");

        if (!typeName.Contains("PageModel"))
            throw new InvalidOperationException(
                $"Unexpected type name '{typeName}' received at PageLocator. Type should end with PageModel.");
        
        return typeName.Replace("Model", string.Empty);
    }
}