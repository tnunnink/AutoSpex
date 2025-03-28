namespace AutoSpex.Engine;

/// <summary>
/// Represents a repository that provides access to storage of source data.
/// </summary>
public class Repo
{
    private static readonly string[] SearchableExtensions = [".L5X", ".ACD", ".L5Z"];
    private const string CacheLocation = @"..\cache";
    private const string CacheName = "cache";

    public Repo()
    {
    }

    /// <summary>
    /// Creates a new repository instance pointing to the provided database location.
    /// </summary>
    public Repo(string location, string? name = null)
    {
        Location = location ?? throw new ArgumentNullException(nameof(location));
        Name = name ?? Path.GetFileNameWithoutExtension(location);
    }

    /// <summary>
    /// Represents the unique identifier of the repository.
    /// </summary>
    public Guid RepoId { get; private init; } = Guid.NewGuid();

    /// <summary>
    /// The location of the repository database file.
    /// </summary>
    public string Location { get; private init; } = @"C:\MyRepo";

    /// <summary>
    /// The name that identifies the repository.
    /// </summary>
    public string Name { get; set; } = "MyRepo";

    /// <summary>
    /// Creates a repo instance named local with the location to the local relative repo database.
    /// All uses will have a default local repo database which should not be deleted.
    /// </summary>
    public static Repo Cache => new() { RepoId = Guid.Empty, Location = CacheLocation, Name = CacheName };


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
}