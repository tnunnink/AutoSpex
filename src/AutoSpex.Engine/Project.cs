using JetBrains.Annotations;

namespace AutoSpex.Engine;

public class Project
{
    [UsedImplicitly]
    private Project()
    {
        Path = default!;
    }
    
    public Project(Uri path)
    {
        Path = path ?? throw new ArgumentNullException(nameof(path));
    }

    public Project(string path)
    {
        if (string.IsNullOrEmpty(path))
            throw new ArgumentException("Can not create Project with null or empty path.");

        Path = new Uri(path);
    }

    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local populated via dapper ORM
    public Uri Path { get; private set; }
    public DateTime OpenedOn { get; set; }
    public bool Pinned { get; set; }
    public string Name => System.IO.Path.GetFileNameWithoutExtension(Path.LocalPath);
    public string Directory => System.IO.Path.GetDirectoryName(Path.LocalPath) ?? string.Empty;
    public bool Exists => File.Exists(Path.LocalPath);
    public string ConnectionString => $"Data Source={Path.LocalPath};Pooling=false;";

    public FileSystemWatcher? CreateWatcher()
    {
        if (!Exists) return null;
        var watcher = new FileSystemWatcher(Directory);
        watcher.EnableRaisingEvents = true;
        watcher.IncludeSubdirectories = false;
        watcher.Filter = System.IO.Path.GetFileName(Path.LocalPath);
        return watcher;
    }
}