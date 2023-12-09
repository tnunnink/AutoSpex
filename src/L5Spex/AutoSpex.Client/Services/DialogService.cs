using System;
using System.Threading.Tasks;
using AutoSpex.Client.Pages;
using Avalonia.Controls;
using JetBrains.Annotations;

namespace AutoSpex.Client.Services;

[UsedImplicitly]
public class DialogService : IDialogService
{
    private readonly Func<Shell> _factory;
    
    public DialogService(Func<Shell> factory)
    {
        _factory = factory;
    }
    
    public Task Show(Control dialog, string? title = default)
    {
        var shell = _factory();
        
        var window = new DialogPage
        {
            Title = title ?? "AutoSpex"
        };
        
        window.InjectContent(dialog);
        return window.ShowDialog(shell);
    }

    public Task<TResult> Show<TResult>(Control dialog, string? title = default)
    {
        var shell = _factory();
        
        var window = new DialogPage
        {
            Title = title ?? "AutoSpex"
        };
        
        window.InjectContent(dialog);
        return window.ShowDialog<TResult>(shell);
    }
}