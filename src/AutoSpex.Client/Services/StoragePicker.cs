using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoSpex.Client.Shared;
using Avalonia.Platform.Storage;
using JetBrains.Annotations;

namespace AutoSpex.Client.Services;

[UsedImplicitly]
public class StoragePicker : IStoragePicker
{
    private IStorageFolder? _previous;
    private readonly Func<IStorageProvider> _factory;

    public StoragePicker(Func<IStorageProvider> factory)
    {
        _factory = factory;
    }

    public async Task<IStorageFolder?> PickFolder(string title)
    {
        var provider = _factory();

        _previous ??= await provider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents);

        var options = new FolderPickerOpenOptions
        {
            Title = title,
            AllowMultiple = false,
            SuggestedStartLocation = _previous
        };

        var folders = await provider.OpenFolderPickerAsync(options);
        if (folders.Count != 1) return default;
        
        var folder = folders[0];
        _previous = folder;
        return folder;
    }

    public async Task<IStorageFile?> PickFile(string title, IReadOnlyList<FilePickerFileType> filters)
    {
        var provider = _factory();

        _previous ??= await provider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents);

        var options = new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = false,
            SuggestedStartLocation = _previous,
            FileTypeFilter = filters
        };

        var files = await provider.OpenFilePickerAsync(options);
        if (files.Count != 1) return default;
        
        var file = files[0];
        _previous = await file.GetParentAsync();
        return file;
    }

    public Task<IStorageFile?> PickProject() => PickFile("Select Spex Project", new[] {ProjectFile});
    public Task<IStorageFile?> PickSource() => PickFile("Select L5X File", new[] {SourceFile});

    public Task<IReadOnlyList<IStorageFile>> PickFiles(Action<FilePickerOpenOptions> config)
    {
        var provider = _factory();

        var options = new FilePickerOpenOptions();
        config(options);

        return provider.OpenFilePickerAsync(options);
    }

    public Task<IReadOnlyList<IStorageFolder>> PickFolders(Action<FolderPickerOpenOptions> config)
    {
        var provider = _factory();

        var options = new FolderPickerOpenOptions();
        config(options);

        return provider.OpenFolderPickerAsync(options);
    }
    
    private static FilePickerFileType ProjectFile { get; } = new("Spex Project")
    {
        Patterns = new[] { $"*{Constant.SpexExtension}" }
    };
    
    private static FilePickerFileType SourceFile { get; } = new("L5X Project")
    {
        Patterns = new[] {"*.L5X", "*.xml"}
    };
}