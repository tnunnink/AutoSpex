using System.Text.Json.Serialization;

namespace AutoSpex.Engine;

/// <summary>
/// A set of data that represents an export for this project.
/// </summary>
public class Package()
{
    /// <summary>
    /// Creates a new <see cref="Package"/> with the provided collection and version.
    /// </summary>
    public Package(Node collection, long version) : this()
    {
        Collection = collection;
        Version = version;
    }

    /// <summary>
    /// The unique identifier for the export package.
    /// </summary>
    [JsonInclude]
    public Guid PackageId { get; private init; } = Guid.NewGuid();

    /// <summary>
    /// Represents the date and time when the package was created.
    /// </summary>
    [JsonInclude]
    public DateTime CreatedOn { get; private init; } = DateTime.Now;

    /// <summary>
    /// The database version this package was exported from;
    /// </summary>
    [JsonInclude]
    public long Version { get; private init; } = 10000;

    /// <summary>
    /// The root node of the <see cref="Package"/>. 
    /// </summary>
    [JsonInclude]
    public Node Collection { get; private init; } = Node.NewCollection();
}