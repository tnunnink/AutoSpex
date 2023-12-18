using System;
using System.Data.SQLite;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoSpex.Client.Features.Projects;

public partial class Project : ObservableObject
{
    public Project(Uri path)
    {
        Uri = path ?? throw new ArgumentNullException(nameof(path));
        File = new FileInfo(Uri.LocalPath);
    }

    public Project(string path, DateTime openedOn)
    {
        Uri = new Uri(path);
        OpenedOn = openedOn;
        File = new FileInfo(Uri.LocalPath);
    }

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(Name))]
    private Uri _uri;

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
        watcher.Renamed += (_, args) => { Uri = new Uri(args.FullPath); };
        return watcher;
    }

    public string ConnectionString => new SQLiteConnectionStringBuilder {DataSource = Uri.LocalPath}.ConnectionString;

    partial void OnUriChanged(Uri value)
    {
        Name = Path.GetFileNameWithoutExtension(value.LocalPath);
    }
}