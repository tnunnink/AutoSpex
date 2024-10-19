using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using L5Sharp.Core;

namespace AutoSpex.Client.Pages;

public partial class NewSourcePageModel : PageViewModel
{
    [ObservableProperty] private FileInfo? _file;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(AddCommand))]
    private SourceObserver? _source;

    [RelayCommand]
    private async Task Browse()
    {
        var path = await Shell.StorageProvider.SelectSourceFile();
        if (path is null) return;
        await LoadSource(path);
    }

    [RelayCommand]
    private async Task Drop(object? data)
    {
        if (data is not IEnumerable<IStorageItem> files) return;
        var file = files.FirstOrDefault();
        if (file is null) return;
        await LoadSource(file.Path);
    }

    [RelayCommand(CanExecute = nameof(CanAdd))]
    private void Add(Window window)
    {
        window.Close(Source);
    }

    private bool CanAdd() => Source is not null;

    /// <summary>
    /// Attempts to read and load the L5X content from the provided Uri path. 
    /// </summary>
    private async Task LoadSource(Uri path)
    {
        try
        {
            var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var content = await L5X.LoadAsync(path.LocalPath, cancellation.Token);
            Source = new Source(content);
            File = new FileInfo(path.LocalPath);
        }
        catch (Exception e)
        {
            Notifier.ShowError("Load Failure", $"Failed to load the selected source file with error {e.Message}.");
        }
    }
}