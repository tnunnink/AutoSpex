using JetBrains.Annotations;

namespace AutoSpex.Engine;

/// <summary>
/// Represents a repository that provides access to storage of source files on disc.
/// </summary>
public class Repo
{
    private static readonly string[] SearchableExtensions = [".L5X", ".ACD", ".L5Z"];
    private readonly HashSet<string> _targets = [];

    [UsedImplicitly]
    private Repo()
    {
        Location = ValidateAndNormalize(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
    }

    /// <summary>
    /// Creates a new repository instance pointing to the provided configuration.
    /// </summary>
    public Repo(string location)
    {
        Location = ValidateAndNormalize(location);
    }

    /// <summary>
    /// Gets the unique identifier for the repository instance.
    /// </summary>
    public Guid RepoId { get; private init; } = Guid.NewGuid();

    /// <summary>
    /// Gets the root directory location where the repository resides.
    /// </summary>
    public string Location { get; }

    /// <summary>
    /// Gets or sets the name of the repository. This will be a client-specific name and not persist in the actual
    /// repo database since we don't really care about the name from there, and we don't want to maintain its state.
    /// </summary>
    public string Name => Path.GetFileName(Location.TrimEnd('\\'));

    /// <summary>
    /// Checks whether the repository is configured by verifying the existence of the root folder location.
    /// </summary>
    public bool Exists => Directory.Exists(Location);

    /// <summary>
    /// 
    /// </summary>
    public IEnumerable<Source> Targets => _targets.Select(Source.Create);

    /// <summary>
    /// Configures and initializes a new repository instance using the specified location path.
    /// </summary>
    /// <param name="location">The file system path where the repository is located or will be created.</param>
    /// <returns>A <see cref="Repo"/> instance initialized at the specified location.</returns>
    public static Repo Configure(string location) => new(location);

    /// <summary>
    /// Determines whether the specified location is one of the repository's targeted sources.
    /// </summary>
    /// <param name="location">The file system path to check against the repository's tracked targets.</param>
    /// <returns>True if the specified location is a targeted source within the repository; otherwise, false.</returns>
    public bool IsTargeted(string location) => _targets.Contains(location);

    /// <summary>
    /// Adds a new target source to the repository's tracked targets.
    /// </summary>
    /// <param name="source">The source to be added to the repository. Must reside within the repository's location.</param>
    /// <exception cref="ArgumentNullException">Thrown if the provided source is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the specified source is not contained within the repository's location.</exception>
    public void AddTarget(Source source)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (!source.Location.StartsWith(Location))
            throw new ArgumentException("The provided source is not contained in this repo.");

        _targets.Add(source.Location);
    }

    /// <summary>
    /// Adds a new target location to the repository's tracked targets.
    /// </summary>
    /// <param name="location">The file system path of the target to be added to the repository. Must reside within the repository's location.</param>
    /// <exception cref="ArgumentException">Thrown if the provided location is null, empty, or not contained within the repository's location.</exception>
    public void AddTarget(string location)
    {
        if (string.IsNullOrEmpty(location))
            throw new ArgumentException("Target cannot be null or empty.");

        if (!location.StartsWith(Location))
            throw new ArgumentException("The provided source is not contained in this repo.");

        _targets.Add(location);
    }

    /// <summary>
    /// Removes the specified source from the repository's targets.
    /// </summary>
    /// <param name="source">The source to be removed from the repository's tracked targets.</param>
    public void RemoveTarget(Source source)
    {
        ArgumentNullException.ThrowIfNull(source);
        _targets.Remove(source.Location);
    }

    /// <summary>
    /// Removes all tracked target sources from the repository.
    /// </summary>
    public void ClearTargets()
    {
        _targets.Clear();
    }

    /// <summary>
    /// Toggles the inclusion of a source in the repository's targeted sources.
    /// If the source is already a target, it will be removed; otherwise, it will be added.
    /// </summary>
    /// <param name="source">The source to toggle in the repository's targeted sources.</param>
    public void ToggleTarget(Source source)
    {
        ArgumentNullException.ThrowIfNull(source);

        //If we can't add, then we remove.
        if (!_targets.Add(source.Location))
        {
            _targets.Remove(source.Location);
        }
    }

    /// <summary>
    /// Finds all source files in the current repository location. This will return the archive, markup, and our custom
    /// compressed file extensions. 
    /// </summary>
    /// <returns>The collection of sources found in the repo.</returns>
    public IEnumerable<Source> FindSources()
    {
        if (!Directory.Exists(Location)) yield break;

        var options = new EnumerationOptions
        {
            IgnoreInaccessible = true,
            RecurseSubdirectories = true,
            MatchType = MatchType.Simple
        };

        var files = SearchableExtensions.AsParallel()
            .SelectMany(extension => Directory.EnumerateFiles(Location, $"*{extension}", options));

        foreach (var file in files)
        {
            yield return Source.Create(file);
        }
    }

    /// <summary>
    /// Validates and normalizes the provided location string for a repository.
    /// </summary>
    /// <param name="location">The file system path to be validated and normalized.</param>
    /// <returns>The validated and normalized version of the input location string.</returns>
    private static string ValidateAndNormalize(string location)
    {
        if (string.IsNullOrWhiteSpace(location))
            throw new ArgumentException("Location cannot be empty", nameof(location));

        if (location.Any(c => Path.GetInvalidPathChars().Contains(c)))
            throw new ArgumentException("Location contains invalid characters", nameof(location));

        return location.TrimEnd('\\');
    }
}