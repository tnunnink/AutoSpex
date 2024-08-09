using System;
using System.Threading.Tasks;
using AutoSpex.Client.Components;
using AutoSpex.Client.Resources.Controls;
using AutoSpex.Client.Shared;
using JetBrains.Annotations;

namespace AutoSpex.Client.Services;

[UsedImplicitly]
[PublicAPI]
public sealed class Prompter(Shell shell)
{
    public static Prompter Default => new(new Shell());

    public async Task<TResult> Show<TResult>(object? content)
    {
        var dialog = new DialogShell { Content = content };

        shell.DialogOpen = true;
        var result = await dialog.ShowDialog<TResult>(shell);
        shell.DialogOpen = false;

        return result;
    }

    public async Task<TResult> Show<TResult>(Func<PageViewModel> factory)
    {
        var page = factory();
        var dialog = new DialogShell { Content = page };
        await page.Load();
        page.IsActive = true;

        shell.DialogOpen = true;
        var result = await dialog.ShowDialog<TResult>(shell);
        shell.DialogOpen = false;

        return result;
    }

    public async Task Show(Func<PageViewModel> factory)
    {
        var page = factory();
        var dialog = new DialogShell { Content = page };
        await page.Load();
        page.IsActive = true;

        shell.DialogOpen = true;
        await dialog.ShowDialog(shell);
        shell.DialogOpen = false;
    }
}

public static class PromptExtensions
{
    public static Task<bool?> PromptError(this Prompter prompter, string title, string message,
        Exception? exception = default)
    {
        var control = new ErrorPrompt { Title = title, ErrorContent = message, Exception = exception };
        return prompter.Show<bool?>(control);
    }

    public static Task<string?> PromptSave(this Prompter prompter, string name)
    {
        var control = new SaveChangesPrompt { ItemName = name };
        return prompter.Show<string?>(control);
    }

    public static Task<bool?> PromptDelete(this Prompter prompter, string message)
    {
        var control = new DeletePrompt { Message = message };
        return prompter.Show<bool?>(control);
    }

    public static Task<string?> PromptRename(this Prompter prompter, Observer observer)
    {
        var control = new NamePrompt
        {
            Title = $"Rename {observer.Icon}",
            Observer = observer
        };
        
        return prompter.Show<string?>(control);
    }
    
    public static Task<string?> PromptNewName(this Prompter prompter, Observer observer)
    {
        var control = new NamePrompt
        {
            Title = $"Duplicate {observer.Icon}",
            Observer = observer
        };
        
        return prompter.Show<string?>(control);
    }
}