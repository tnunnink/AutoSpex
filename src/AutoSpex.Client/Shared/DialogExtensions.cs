using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using FluentResults;

namespace AutoSpex.Client.Shared;

public static class DialogExtensions
{
    private static FilePickerFileType Spex { get; } = new("Spex Project")
    {
        Patterns = new[] { "*.spex" }
    };

    private static FilePickerFileType L5X { get; } = new("L5X")
    {
        Patterns = new[] { "*.L5X", "*.l5x" }
    };

    public static async Task<Uri?> SelectFolderUri(this IStorageProvider provider, string title)
    {
        var options = new FolderPickerOpenOptions
        {
            Title = title,
            AllowMultiple = false
        };

        var folder = (await provider.OpenFolderPickerAsync(options)).SingleOrDefault();
        return folder?.Path;
    }

    public static async Task<Uri?> SelectSourceFile(this IStorageProvider provider)
    {
        var options = new FilePickerOpenOptions
        {
            Title = "Select Source L5X",
            AllowMultiple = false,
            FileTypeFilter = [L5X]
        };

        var folder = (await provider.OpenFilePickerAsync(options)).SingleOrDefault();
        return folder?.Path;
    }

    public static Task ShowInExplorer(this IStorageProvider service, string directory)
    {
        return Task.FromResult(Result.Try(() =>
        {
            Process.Start(new ProcessStartInfo { FileName = directory, UseShellExecute = true });
        }));
    }
}