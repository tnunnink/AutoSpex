using JetBrains.Annotations;

namespace AutoSpex.Engine;

/// <summary>
/// Represents a repository that provides access to storage of source files on disc.
/// </summary>
public class Repo
{
    private static readonly string AppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    private const string AppFolder = "AutoSpex";
    private const string LocalRepo = "local";
    private static readonly string[] SearchableExtensions = [".L5X", ".ACD", ".L5Z"];
    private const string RootFolder = ".spex";
    private const string DatabaseName = "repo.db";
    private const string CacheFolder = "cache";

    private readonly string _cacheLocation;

    [UsedImplicitly]
    private Repo()
    {
        Location = Path.Combine(AppData, AppFolder, LocalRepo);
        Name = Path.GetFileNameWithoutExtension(Location);
        ConnectionString = Path.Combine(Location, RootFolder, DatabaseName);
        _cacheLocation = Path.Combine(Location, RootFolder, CacheFolder);
    }

    /// <summary>
    /// Creates a new repository instance pointing to the provided database location.
    /// </summary>
    public Repo(string location, string? name = null)
    {
        Location = location ?? throw new ArgumentNullException(nameof(location));
        Name = name ?? Path.GetFileNameWithoutExtension(location);
        ConnectionString = Path.Combine(Location, RootFolder, DatabaseName);
        _cacheLocation = Path.Combine(Location, RootFolder, CacheFolder);
    }

    /// <summary>
    /// Represents the unique identifier of the repository.
    /// </summary>
    public Guid RepoId { get; private init; } = Guid.NewGuid();

    /// <summary>
    /// The location of the repository directory. This will be the root directory in which we seed our
    /// sidecar files and scan subdirectories for source files.
    /// </summary>
    public string Location { get; private init; }

    /// <summary>
    /// The name that identifies the repository.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Represents the connection string to the database associated with the repository.
    /// The connection string is formed by combining the repository location, root folder, and database name.
    /// </summary>
    /// <remarks>
    /// The connection string is used to establish a connection to the database for storing and retrieving data.
    /// </remarks>
    public string ConnectionString { get; }

    /// <summary>
    /// Builds a new repository instance at the specified location by creating the necessary directory structure.
    /// </summary>
    /// <param name="location">The location where the repository will be created.</param>
    /// <returns>A new Repo instance pointing to the provided location.</returns>
    public static Repo Build(string location)
    {
        BuildRepoStructure(location);
        return new Repo(location);
    }

    /// <summary>
    /// Checks whether the repository is configured by verifying the existence of the root folder in the provided location.
    /// </summary>
    /// <returns>
    /// True if the repository is configured (root folder exists), otherwise false.
    /// </returns>
    public bool IsConfigured() => Directory.Exists(Path.Combine(Location, RootFolder));

    /// <summary>
    /// Searches for and retrieves all sources in the repository that match predefined searchable file extensions.
    /// </summary>
    /// <returns>
    /// A collection of Source objects representing the found files, or an empty collection if no matching files are located.
    /// </returns>
    public IEnumerable<Source> FindSources()
    {
        if (!Directory.Exists(Location)) yield break;

        var options = new EnumerationOptions
        {
            IgnoreInaccessible = true,
            RecurseSubdirectories = true,
            MatchType = MatchType.Simple
        };

        foreach (var extension in SearchableExtensions)
        {
            var files = Directory.EnumerateFiles(Location, $"*{extension}", options);

            foreach (var file in files)
            {
                yield return Source.Create(file);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void Rebuild()
    {
        Directory.Delete(Path.Combine(Location, RootFolder), true);
        BuildRepoStructure(Location);
    }

    /// <summary>
    /// 
    /// </summary>
    public void Delete()
    {
        Directory.Delete(Path.Combine(Location, RootFolder), true);
    }

    /// <summary>
    /// Caches the specified source asynchronously if it is not already cached.
    /// </summary>
    /// <param name="source">The source object to be cached.</param>
    /// <param name="token">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous caching operation.</returns>
    public async Task Cache(Source source, CancellationToken token = default)
    {
        if (IsCached(source)) return;
        await CacheSource(source, token);
    }

    /// <summary>
    /// Retrieves the specified source from the repository if it is already cached;
    /// otherwise, adds it to the cache by compressing its contents and saving it to a file.
    /// </summary>
    /// <param name="source">The source object to be retrieved or cached.</param>
    /// <param name="token">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the retrieved or newly cached source instance.
    /// </returns>
    public Task<Source> GetOrCache(Source source, CancellationToken token = default)
    {
        return TryGetCachedSource(source, out var cached) ? Task.FromResult(cached) : CacheSource(source, token);
    }

    /// <summary>
    /// Removes all cached files of the Compressed source type from the repository's location.
    /// This method enumerates the files in the repository directory matching the Compressed source file extension
    /// and deletes them in order to free up storage or clear outdated cache entries.
    /// </summary>
    public void ClearCache()
    {
        var files = Directory.EnumerateFiles(_cacheLocation, $"*{SourceType.Compressed.Extension}");

        foreach (var file in files)
        {
            File.Delete(file);
        }
    }

    /// <summary>
    /// Caches the specified source in the repository by compressing its contents and saving it to a file.
    /// </summary>
    /// <param name="source">The source object to be cached.</param>
    /// <param name="token">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the newly cached source instance created from the saved file.
    /// </returns>
    private async Task<Source> CacheSource(Source source, CancellationToken token)
    {
        var fileName = Path.Combine(_cacheLocation, $"{source.Hash}.L5Z");
        var content = await source.OpenAsync(token);
        await content.Serialize().ToString().CompressToAsync(fileName, token);
        return Source.Create(fileName);
    }

    /// <summary>
    /// Attempts to retrieve a cached source matching the specified source's hash from the repository.
    /// </summary>
    /// <param name="source">The source object used to identify the cached source in the repository.</param>
    /// <param name="cached">When this method returns, contains the cached source if it exists; otherwise, null.</param>
    /// <returns>
    /// true if a cached source matching the specified source's hash is found in the repository; otherwise, false.
    /// </returns>
    private bool TryGetCachedSource(Source source, out Source cached)
    {
        var files = Directory.EnumerateFiles(_cacheLocation, $"*{SourceType.Compressed.Extension}");

        foreach (var file in files)
        {
            var hash = Path.GetFileNameWithoutExtension(file);
            if (hash != source.Hash) continue;
            cached = Source.Create(file);
            return true;
        }

        cached = null!;
        return false;
    }

    /// <summary>
    /// Determines whether the specified source is already cached within the repository.
    /// </summary>
    /// <param name="source">The source to check for existence in the cache.</param>
    /// <returns>True if the source is cached; otherwise, false.</returns>
    private bool IsCached(Source source)
    {
        var files = Directory.EnumerateFiles(_cacheLocation, $"*{SourceType.Compressed.Extension}");

        foreach (var file in files)
        {
            var hash = Path.GetFileNameWithoutExtension(file);
            if (hash == source.Hash) return true;
        }

        return false;
    }

    /// <summary>
    /// Builds the required directory structure for a new repository at the specified location.
    /// </summary>
    /// <param name="location">The directory path where the repository structure will be built.</param>
    private static void BuildRepoStructure(string location)
    {
        if (string.IsNullOrEmpty(location))
            throw new ArgumentException("Can not build repo without a directory location");

        //We only allow single root repo directories, so check for a parent repo and throw if found.
        var parentRepo = FindParentRepoPath(location);
        if (parentRepo is not null)
            throw new InvalidOperationException($"A repo already exists at '{parentRepo}'");

        //Will ensure the location is created to the nested path if not already.
        var repoPath = Path.Combine(location, RootFolder);
        Directory.CreateDirectory(repoPath);

        var cachePath = Path.Combine(repoPath, CacheFolder);
        Directory.CreateDirectory(cachePath);
    }

    /// <summary>
    /// Finds the parent repository path of the provided location by recursively checking up the directory structure.
    /// </summary>
    /// <param name="path">The location for which to find the parent repository path.</param>
    /// <returns>The path of the parent repository if found, otherwise null.</returns>
    private static string? FindParentRepoPath(string path)
    {
        var current = Path.GetFullPath(path);

        while (!string.IsNullOrEmpty(current))
        {
            if (Directory.Exists(Path.Combine(current, RootFolder))) return current;
            var parent = Path.GetDirectoryName(current);
            if (parent == current || parent == null) break;
            current = parent;
        }

        return null;
    }
}