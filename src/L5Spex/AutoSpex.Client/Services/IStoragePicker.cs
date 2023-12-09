using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace AutoSpex.Client.Services;

public interface IStoragePicker
{
    Task<IStorageFolder?> PickFolder(string title);
    
    Task<IStorageFile?> PickFile(string title, IReadOnlyList<FilePickerFileType> filters);
    
    Task<IStorageFile?> PickProject();
    
    Task<IStorageFile?> PickSource();
    
    Task<IReadOnlyList<IStorageFile>> PickFiles(Action<FilePickerOpenOptions> config);
    
    Task<IReadOnlyList<IStorageFolder>> PickFolders(Action<FolderPickerOpenOptions> config);
    
}