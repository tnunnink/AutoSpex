using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AutoSpex.Client.Pages;

public partial class OpenPackagePageModel : PageViewModel
{
    [ObservableProperty] private FileInfo? _file;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(ImportCommand))]
    private Package? _package;

    [RelayCommand]
    private async Task Browse()
    {
        var path = await Shell.StorageProvider.SelectImportFile();
        if (path is null) return;
        await OpenPackage(path);
    }

    [RelayCommand]
    private async Task Drop(object? data)
    {
        if (data is not IEnumerable<IStorageItem> files) return;
        var file = files.FirstOrDefault();
        if (file is null) return;
        await OpenPackage(file.Path);
    }

    [RelayCommand(CanExecute = nameof(CanImport))]
    private void Import(Window window)
    {
        window.Close(Package);
    }

    private bool CanImport() => Package is not null;


    /// <summary>
    /// Attempts to read and parse the JSON content into a Package and sets the local property. 
    /// </summary>
    private async Task OpenPackage(Uri path)
    {
        try
        {
            using var reader = new StreamReader(path.LocalPath);
            var content = await reader.ReadToEndAsync();
            var package = JsonSerializer.Deserialize<Package>(content);

            //Unable to parse the file...
            if (package is null)
            {
                Notifier.ShowError(
                    "Import Error",
                    "File format is not recognized/supported. Please import a .json file that was exported using AutoSpex."
                );
            }

            Package = package;
            File = new FileInfo(path.LocalPath);
        }
        catch (Exception e)
        {
            Notifier.ShowError("Import Error", $"Open package failed with error {e.Message}.");
        }
    }
}