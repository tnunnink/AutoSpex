using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;

namespace L5Spex.Services;

public interface IFileService
{
    public Task<IStorageFile> OpenFileAsync();
    public Task<IStorageFile> SaveFileAsync();
}

public class FileService : IFileService
{
    private readonly IClassicDesktopStyleApplicationLifetime _desktop;

    public FileService(IClassicDesktopStyleApplicationLifetime desktop)
    {
        this._desktop = desktop ?? throw new ArgumentNullException(nameof(desktop));
    }

    public async Task<IStorageFile> OpenFileAsync()
    {
        var target = _desktop.MainWindow ??
                     throw new InvalidOperationException("No top level storage provider found.");
        
        var files = await target.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Text File",
            AllowMultiple = false
        });

        return files.Count >= 1 ? files[0] : null;
    }
    
    public async Task<IStorageFile> OpenFileAsync(string title, IEnumerable<string> filters)
    {
        var target = _desktop.MainWindow ??
                     throw new InvalidOperationException("No top level storage provider found.");
        
        var files = await target.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = title,
            FileTypeFilter = filters.Select(s => new FilePickerFileType(s)).ToList()
        });

        return files.Count >= 1 ? files[0] : null;
    }

    public async Task<IStorageFile> SaveFileAsync()
    {
        var target = _desktop.MainWindow ??
                     throw new InvalidOperationException("No top level storage provider found.");
        
        return await target.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions()
        {
            Title = "Save Text File"
        });
    }
}