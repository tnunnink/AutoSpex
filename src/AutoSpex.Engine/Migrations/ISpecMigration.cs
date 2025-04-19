namespace AutoSpex.Engine.Migrations;

/// <summary>
/// Represents an interface for specifying migration logic for old data.
/// </summary>
public interface ISpecMigration
{
    /// <summary>
    /// Gets the version of the spec migration.
    /// </summary>
    int Version { get; }

    /// <summary>
    /// Migrates the specified JSON data based on the implemented migration logic.
    /// </summary>
    /// <param name="json">The JSON data that needs to be migrated.</param>
    /// <returns>The new raw JSON string with the correct schema for the specified version.</returns>
    string Run(string json);
}