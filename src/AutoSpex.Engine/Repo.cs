using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace AutoSpex.Engine;

/// <summary>
/// Represents a repository that provides access to storage of source files on disc.
/// </summary>
public class Repo
{
    private static readonly string[] SearchableExtensions = [".L5X", ".ACD", ".L5Z"];

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
    /// Gets or sets the name of the repository. This will be a client specific name, and not persisted in the actual
    /// repo database since we don't really care about the name from there, and we don't want to maintain it's state.
    /// </summary>
    public string Name => Path.GetFileName(Location.TrimEnd('\\'));

    /// <summary>
    /// Checks whether the repository is configured by verifying the existence of the root folder location.
    /// </summary>
    public bool Exists => Directory.Exists(Location);

    /// <summary>
    /// Configures and initializes a new repository instance using the specified location path.
    /// </summary>
    /// <param name="location">The file system path where the repository is located or will be created.</param>
    /// <returns>A <see cref="Repo"/> instance initialized at the specified location.</returns>
    public static Repo Configure(string location) => new(location);

    /// <summary>
    /// Finds all source files in the current repository location. This wil return archive, markup, and our custom
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