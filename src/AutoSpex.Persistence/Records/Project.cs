namespace AutoSpex.Persistence;

public class Project
{
    private readonly FileInfo _file;
    
    public Project(Uri path)
    {
        Uri = path ?? throw new ArgumentNullException(nameof(path));
        _file = new FileInfo(Uri.LocalPath);
    }

    public Project(string path)
    {
        if (string.IsNullOrEmpty(path))
            throw new ArgumentException("Can not create Project with null or empty path.");
        
        Uri = new Uri(path);
        _file = new FileInfo(Uri.LocalPath);
    }

    public Uri Uri { get; }
    public DateTime OpenedOn { get; set; }
    public string Name => Path.GetFileNameWithoutExtension(Uri.LocalPath);
    public string Directory => Path.GetDirectoryName(Uri.LocalPath) ?? string.Empty;
    public bool Exists => _file.Exists;
    public string ConnectionString => $"Data Source={Uri.LocalPath};Pooling=false;";

    public FileSystemWatcher CreateWatcher()
    {
        var watcher = new FileSystemWatcher(Directory);
        watcher.EnableRaisingEvents = true;
        watcher.IncludeSubdirectories = false;
        watcher.Filter = Name;
        return watcher;
    }
}