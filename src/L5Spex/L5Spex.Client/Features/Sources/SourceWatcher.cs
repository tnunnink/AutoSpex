using System;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;

namespace L5Spex.Client.Features.Sources;

public class SourceWater : ObservableObject
{
    
    private readonly FileInfo _fileInfo;
    private readonly FileSystemWatcher _fileSystemWatcher;

    public SourceWater() : this(new SourceRecord("C:/Path/To/Source/Source Name.L5X"))
    {
    }

    public SourceWater(SourceRecord record)
    {
        Record = record ?? throw new ArgumentNullException(nameof(record));
        _fileInfo = new FileInfo(Record.Path);
        _fileSystemWatcher = new FileSystemWatcher(_fileInfo.DirectoryName!, _fileInfo.Name);
        _fileSystemWatcher.Created += OnFileChanged;
        _fileSystemWatcher.Deleted += OnFileChanged;
        _fileSystemWatcher.Renamed += OnFileChanged;
        _fileSystemWatcher.EnableRaisingEvents = true;
    }

    public SourceWater(string path)
    {
        if (string.IsNullOrEmpty(path)) throw new ArgumentException(null, nameof(path));

        _fileInfo = new FileInfo(path);
        _fileSystemWatcher = new FileSystemWatcher(_fileInfo.DirectoryName!, _fileInfo.Name);
        _fileSystemWatcher.Created += OnFileChanged;
        _fileSystemWatcher.Deleted += OnFileChanged;
        _fileSystemWatcher.Renamed += OnFileChanged;
        _fileSystemWatcher.EnableRaisingEvents = true;
    }

    public SourceRecord Record { get; }
    public string FullName => _fileInfo.FullName;
    public string ProjectName => Path.GetFileNameWithoutExtension(_fileInfo.Name);
    public string FileName => _fileInfo.Name;
    public string DirectoryName => _fileInfo.DirectoryName;
    public bool Exists => _fileInfo.Exists;

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        _fileInfo.Refresh();
        OnPropertyChanged(string.Empty);
    }
}