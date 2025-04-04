using System.Text.Json.Serialization;

namespace AutoSpex.Engine;

/// <summary>
/// Represents a repository that provides access to storage of source files on disc.
/// </summary>
public class Repo
{
    private static readonly string[] SearchableExtensions = [".L5X", ".ACD", ".L5Z"];

    [JsonConstructor]
    private Repo(Guid repoId, string location, string name)
    {
        ValidateLocation(location);

        RepoId = repoId;
        Location = location;
        Name = name;
    }

    /// <summary>
    /// Creates a new repository instance pointing to the provided configuration.
    /// </summary>
    public Repo(string location, string? name = null)
    {
        ValidateLocation(location);
        Location = location;
        Name = name ?? Path.GetFileNameWithoutExtension(location);
    }

    /// <summary>
    /// Gets the unique identifier for the repository instance.
    /// </summary>
    [JsonInclude]
    public Guid RepoId { get; private init; } = Guid.NewGuid();

    /// <summary>
    /// Gets the root directory location where the repository resides.
    /// </summary>
    [JsonInclude]
    public string Location { get; }

    /// <summary>
    /// Gets or sets the name of the repository. This will be a client specific name, and not persisted in the actual
    /// repo database since we don't really care about the name from there, and we don't want to maintain it's state.
    /// </summary>
    [JsonInclude]
    public string Name { get; set; }

    /// <summary>
    /// Checks whether the repository is configured by verifying the existence of the root folder location.
    /// </summary>
    [JsonIgnore]
    public bool Exists => Directory.Exists(Location);

    /// <summary>
    /// Configures and initializes a new repository instance using the specified location path.
    /// </summary>
    /// <param name="location">The file system path where the repository is located or will be created.</param>
    /// <returns>A <see cref="Repo"/> instance initialized at the specified location.</returns>
    public static Repo Configure(string location) => new(location);

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
    /// Builds the repository by creating the necessary directory structure for storage.
    /// </summary>
    /// <returns>The current Repo instance after successfully building the repository.</returns>
    /// <exception cref="InvalidOperationException">Thrown if a parent repository is found at the specified location.</exception>
    public Repo Build()
    {
        Directory.CreateDirectory(Location);
        return this;
    }

    /// <summary>
    /// Validates the provided location to ensure it is a valid path for the repository database.
    /// </summary>
    /// <param name="location">The location to be validated.</param>
    /// <exception cref="ArgumentException">Thrown when the location is empty or contains invalid characters.</exception>
    private static void ValidateLocation(string location)
    {
        if (string.IsNullOrWhiteSpace(location))
            throw new ArgumentException("Location cannot be empty", nameof(location));

        if (location.Any(c => Path.GetInvalidPathChars().Contains(c)))
            throw new ArgumentException("Location contains invalid characters", nameof(location));
    }
}