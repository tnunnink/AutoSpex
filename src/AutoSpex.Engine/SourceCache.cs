namespace AutoSpex.Engine;

/// <summary>
/// Provides functionality to manage and retrieve cached source objects.
/// Handles caching by compressing and storing source contents in the file system,
/// and retrieves them when needed to avoid redundant processing.
/// </summary>
public class SourceCache
{
    private const string CacheDirectory = "cache";

    /// <summary>
    /// Creates a new <see cref="SourceCache"/> instance using the provided repo as the location in
    /// which to cache sources.
    /// </summary>
    private SourceCache(Repo repo)
    {
        Repo = repo ?? throw new ArgumentNullException(nameof(repo));
        Directory.CreateDirectory(Path.Combine(Repo.Location, CacheDirectory));
    }

    /// <summary>
    /// Gets the repository instance associated with the current cache,
    /// which provides access to the underlying storage of source data.
    /// </summary>
    public Repo Repo { get; }

    /// <summary>
    /// 
    /// </summary>
    public static SourceCache In(Repo repo) => new(repo);

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
    public Task<Source> GetOrAdd(Source source, CancellationToken token = default)
    {
        return TryGetSource(source, out var cached) ? Task.FromResult(cached) : CacheSource(source, token);
    }

    /// <summary>
    /// Removes all cached files of the Compressed source type from the repository's location.
    /// This method enumerates the files in the repository directory matching the Compressed source file extension
    /// and deletes them in order to free up storage or clear outdated cache entries.
    /// </summary>
    public void ClearCache()
    {
        var files = Directory.EnumerateFiles(Repo.Location, $"*{SourceType.Compressed.Extension}");

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
        var fileName = Path.Combine(Repo.Location, $"{source.Hash}.L5Z");
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
    private bool TryGetSource(Source source, out Source cached)
    {
        var files = Directory.EnumerateFiles(Repo.Location, $"*{SourceType.Compressed.Extension}");

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
        var files = Directory.EnumerateFiles(Repo.Location, $"*{SourceType.Compressed.Extension}");

        foreach (var file in files)
        {
            var hash = Path.GetFileNameWithoutExtension(file);
            if (hash == source.Hash) return true;
        }

        return false;
    }
}