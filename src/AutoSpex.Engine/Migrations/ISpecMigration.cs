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
    /// Migrates the specified old data based on the implemented migration logic.
    /// </summary>
    /// <param name="old">The old data that needs to be migrated.</param>
    /// <returns>The migrated data.</returns>
    string Run(string old);
}