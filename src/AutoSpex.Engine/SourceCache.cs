namespace AutoSpex.Engine;

/// <summary>
/// Provides functionality to manage and retrieve cached source objects.
/// Handles caching by compressing and storing source contents in the file system,
/// and retrieves them when needed to avoid redundant processing.
/// </summary>
public class SourceCache
{
    private static readonly string AppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    private const string AppFolder = "AutoSpex";
    private const string CacheRepo = "cache";
    private readonly Repo _repo;

    /// <summary>
    /// Creates a new <see cref="SourceCache"/> instance using the provided repo as the location in
    /// which to cache sources.
    /// </summary>
    public SourceCache(Repo repo)
    {
        _repo = repo ?? throw new ArgumentNullException(nameof(repo));
    }

    /// <summary>
    /// Gets a static instance of the <see cref="SourceCache"/> class that operates
    /// on the local user's app data repository.
    /// </summary>
    public static SourceCache Local => new(Repo.Configure(Path.Combine(AppData, AppFolder, CacheRepo)));

    /// <summary>
    /// Gets a collection of <see cref="Source"/> objects retrieved from the repository.
    /// The retrieved sources represent cached or discoverable files that match
    /// the predefined criteria in the current caching context.
    /// </summary>
    public IEnumerable<Source> Sources => _repo.FindSources();

    /// <summary>
    /// Caches the specified source asynchronously if it is not already cached.
    /// </summary>
    /// <param name="source">The source object to be cached.</param>
    /// <param name="token">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous caching operation.</returns>
    public async Task Cache(Source source, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        _repo.Build();

        if (IsSourceCached(source)) return;
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
    public Task<Source> GetOrAdd(Source source, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        _repo.Build();

        return TryGetCachedSource(source, out var cached)
            ? Task.FromResult(cached)
            : CacheSource(source, token);
    }

    /// <summary>
    /// Deletes all cached folders/files from the repository's location.
    /// </summary>
    public void Clear()
    {
        if (!Directory.Exists(_repo.Location)) return;

        var folders = Directory.EnumerateDirectories(_repo.Location);

        foreach (var file in folders)
        {
            Directory.Delete(file, true);
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
        //Prep the cache folder for this source be creating the info object, deleting current directory if it exists,
        //and then recreating the new empty directory. 
        var cacheFolder = new DirectoryInfo(Path.Combine(_repo.Location, source.FileHash));
        if (cacheFolder.Exists) cacheFolder.Delete(true);
        cacheFolder.Create();

        //Compute the new file path for the cache compressed content file 
        var cacheFile = Path.Combine(_repo.Location, cacheFolder.Name, $"{source.ContentHash}.L5Z");

        //Compress the source content and save it to the cache file
        var content = await source.OpenAsync(token);
        await content.Serialize().ToString().CompressToAsync(cacheFile, token);

        //Return the new cached source instance to reference instead of the actual provided source.
        return Source.Create(cacheFile);
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
        var folders = Directory.EnumerateDirectories(_repo.Location);

        foreach (var folder in folders)
        {
            if (Path.GetFileName(folder) != source.FileHash) continue;

            var cachedFile = Path.Combine(folder, $"{source.ContentHash}.L5Z");
            if (!File.Exists(cachedFile)) break;

            cached = Source.Create(cachedFile);
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
    private bool IsSourceCached(Source source)
    {
        var cacheFolder = Path.Combine(_repo.Location, source.FileHash);
        var cacheFile = Path.Combine(cacheFolder, $"{source.ContentHash}.L5Z");
        return File.Exists(cacheFile);
    }
}