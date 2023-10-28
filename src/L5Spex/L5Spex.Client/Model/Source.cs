using System;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;

namespace L5Spex.Client.Model;

public class Source : ObservableObject
{
    private readonly FileInfo _fileInfo;
    private readonly FileSystemWatcher _fileSystemWatcher;

    public Source()
    {
        SourceId = Guid.NewGuid();
        SourcePath = "C:/Path/To/Source/Source Name.L5X";
        Selected = false;
        
        _fileInfo = new FileInfo(SourcePath);
        _fileSystemWatcher = new FileSystemWatcher(_fileInfo.DirectoryName!, _fileInfo.Name);
        _fileSystemWatcher.Created += OnFileChanged;
        _fileSystemWatcher.Deleted += OnFileChanged;
        _fileSystemWatcher.Renamed += OnFileChanged;
        _fileSystemWatcher.EnableRaisingEvents = true;
    }

    public Source(Uri path)
    {
        if (path is null) 
            throw new ArgumentException(null, nameof(path));

        SourceId = Guid.NewGuid();
        SourcePath = path.LocalPath;
        Selected = false;
        
        _fileInfo = new FileInfo(SourcePath);
        _fileSystemWatcher = new FileSystemWatcher(_fileInfo.DirectoryName!, _fileInfo.Name);
        _fileSystemWatcher.Created += OnFileChanged;
        _fileSystemWatcher.Deleted += OnFileChanged;
        _fileSystemWatcher.Renamed += OnFileChanged;
        _fileSystemWatcher.EnableRaisingEvents = true;
    }

    public Guid SourceId { get; private set; }
    public string SourcePath { get; private set; }
    public bool Selected { get; set; }
    public string FileName => _fileInfo.Name;
    public string DirectoryName => _fileInfo.DirectoryName;
    public bool Exists => _fileInfo.Exists;

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        _fileInfo.Refresh();
        OnPropertyChanged(string.Empty);
    }

    public override string ToString() => SourcePath;
}