using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoSpex.Engine;
using Avalonia.Platform.Storage;
using FluentResults;

namespace AutoSpex.Client.Shared;

public static class DialogExtensions
{
    private static FilePickerFileType Json { get; } = new("JSON File")
    {
        Patterns = new[] { "*.json" }
    };

    private static FilePickerFileType L5X { get; } = new("L5X File")
    {
        Patterns = new[] { "*.L5X", "*.l5x" }
    };

    /// <summary>
    /// Opens the file picker with options to select a source L5X file to be added to the application.
    /// </summary>
    /// <param name="provider">The storage provider service.</param>
    /// <returns>The <see cref="Uri"/> of the selected file.</returns>
    public static async Task<Uri?> SelectSourceFile(this IStorageProvider provider)
    {
        var options = new FilePickerOpenOptions
        {
            Title = "Select Source L5X",
            AllowMultiple = false,
            FileTypeFilter = [L5X]
        };

        var file = (await provider.OpenFilePickerAsync(options)).SingleOrDefault();
        return file?.Path;
    }
    
    /// <summary>
    /// Opens the file picker with options to select a import JSON file to be imported to the application.
    /// </summary>
    /// <param name="provider">The storage provider service.</param>
    /// <returns>The <see cref="Uri"/> of the selected file.</returns>
    public static async Task<Uri?> SelectImportFile(this IStorageProvider provider)
    {
        var options = new FilePickerOpenOptions
        {
            Title = "Select file to import",
            AllowMultiple = false,
            FileTypeFilter = [Json]
        };

        var file = (await provider.OpenFilePickerAsync(options)).SingleOrDefault();
        return file?.Path;
    }

    /// <summary>
    /// Writes the provided package data as a serialized Json document to the selected storage destination.
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="package"></param>
    /// <returns></returns>
    public static async Task<Result> ExportPackage(this IStorageProvider provider, Package package)
    {
        var invalidChars = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
        var fileName = invalidChars.Aggregate(package.Collection.Name, (current, c) => current.Replace(c.ToString(), ""));

        var options = new FilePickerSaveOptions
        {
            Title = "Export Package",
            DefaultExtension = ".json",
            FileTypeChoices = [Json],
            SuggestedFileName = fileName,
            ShowOverwritePrompt = true,
        };

        var file = await provider.SaveFilePickerAsync(options);
        if (file is null) return Result.Ok();

        var content = JsonSerializer.Serialize(package, new JsonSerializerOptions { WriteIndented = true });

        try
        {
            var stream = await file.OpenWriteAsync();
            await using var writer = new StreamWriter(stream);
            await writer.WriteAsync(content);
            return Result.Ok();
        }
        catch (Exception e)
        {
            return Result.Fail(e.Message);
        }
    }

    // ReSharper disable once UnusedParameter.Global still want to use as storage provider extension
    public static Task ShowInExplorer(this IStorageProvider service, string directory)
    {
        return Task.FromResult(Result.Try(() =>
        {
            Process.Start(new ProcessStartInfo { FileName = directory, UseShellExecute = true });
        }));
    }
}