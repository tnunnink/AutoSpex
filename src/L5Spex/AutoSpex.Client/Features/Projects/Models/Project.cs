using System;
using System.Data.SQLite;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoSpex.Client.Features.Projects;

public partial class Project : ObservableObject
{
    public Project(Uri path)
    {
        Path = path ?? throw new ArgumentNullException(nameof(path));
        File = new FileInfo(Path.AbsolutePath);
    }

    public Project(dynamic record)
    {
        Path = new Uri(record.Path);
        OpenedOn = record.OpenedOn;
        Pinned = record.Pinned == 1;
        File = new FileInfo(Path.AbsolutePath);
    }

    /*public static implicit operator Project(object record) => new(record);*/

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(Name))]
    private Uri _path;

    [ObservableProperty] private bool _pinned;

    [ObservableProperty] private DateTime _openedOn = DateTime.UtcNow;

    [ObservableProperty] private string _name = string.Empty;

    public FileInfo File { get; }

    public FileSystemWatcher GetWatcher()
    {
        var watcher =
            new FileSystemWatcher(File.DirectoryName ??
                                  throw new InvalidOperationException("File.DirectoryName is null"));
        watcher.EnableRaisingEvents = true;
        watcher.IncludeSubdirectories = false;
        watcher.Filter = File.Name;
        watcher.Renamed += (_, args) => { Path = new Uri(args.FullPath); };
        return watcher;
    }

    public string ConnectionString => new SQLiteConnectionStringBuilder {DataSource = Path.LocalPath}.ConnectionString;

    partial void OnPathChanged(Uri value)
    {
        Name = System.IO.Path.GetFileNameWithoutExtension(value.AbsolutePath);
    }
}