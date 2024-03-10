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
        var dialog = new DialogShell {Content = content};

        shell.DialogOpen = true;
        var result = await dialog.ShowDialog<TResult>(shell);
        shell.DialogOpen = false;

        return result;
    }
    
    public async Task<TResult> Show<TResult>(Func<PageViewModel> factory)
    {
        var page = factory();
        var dialog = new DialogShell {Content = page};
        await page.Load();

        shell.DialogOpen = true;
        var result = await dialog.ShowDialog<TResult>(shell);
        shell.DialogOpen = false;

        return result;
    }
}

public static class PromptExtensions
{
    public static Task<bool?> PromptDelete(this Prompter prompter, string name)
    {
        var control = new DeletePrompt {ItemName = name};
        return prompter.Show<bool?>(control);
    }
    
    public static Task<string?> PromptSave(this Prompter prompter, string name)
        {
            var control = new SaveChangesPrompt {ItemName = name};
            return prompter.Show<string?>(control);
        }
}