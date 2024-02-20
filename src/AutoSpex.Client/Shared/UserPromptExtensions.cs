using System.Windows.Input;
using ActiproSoftware.UI.Avalonia.Controls;

namespace AutoSpex.Client.Shared;

public static class UserPromptExtensions
{
    public static UserPromptBuilder SaveChangesPrompt(this UserPromptBuilder builder, 
        string title, bool prompt, ICommand save)
    {
        return builder
            .WithHeaderContent("Do you want to save changes?")
            .WithContent(
                $"The page {title} has unsaved changes which will be lost if you choose to close it. Save these changes to avoid losing your work.")
            .WithCheckBoxContent("Always discard unsaved changes when closing a tab")
            .WithIsChecked(() => !prompt, v => prompt = !v)
            .WithButton(c =>
            {
                c.WithContent("Save Changes")
                    .WithCommand(save)
                    .WithClasses("theme-soft warning")
                    .WithResult(MessageBoxResult.Yes);
            })
            .WithButton(c =>
            {
                c.WithContent("Don't Save")
                    .WithClasses("theme-soft")
                    .WithResult(MessageBoxResult.Yes)
                    .UseAsDefaultResult();
            })
            .WithButton(c =>
            {
                c.WithContent("Cancel")
                    .WithClasses("theme-soft")
                    .UseAsCloseResult();
            });
    }
}