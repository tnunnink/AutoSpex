using System.Data;
using FluentResults;

namespace AutoSpex.Persistence;

/// <summary>
/// An interface for managing the database connection for this layer. This interface allows connection, migration,
/// and registration. External classes don't really need to worry about this because it is only required for the persistence
/// layer requests. This will be implemented as a singleton so the registration of data sources can be consistent in
/// every transient request.
/// </summary>
public interface IConnectionManager
{
    /// <summary>
    /// Registers the specified database with the given data source.
    /// </summary>
    /// <param name="database">The database object to register.</param>
    /// <param name="dataSource">The data source to register the database with.</param>
    void Register(Database database, string dataSource);
    
    /// <summary>
    /// Determines if the specified database is registered.
    /// </summary>
    /// <param name="database">The database to check.</param>
    /// <returns>True if the database is registered; otherwise, false.</returns>
    bool IsRegistered(Database database);

    /// <summary>
    /// Retrieves the source of the specified database.
    /// </summary>
    /// <param name="database">The database for which to retrieve the source.</param>
    /// <returns>The source of the specified database as a string.</returns>
    string GetSource(Database database);
    
    /// <summary>
    /// Opens a connection to the specified database and returns the new <see cref="IDbConnection"/> instance.
    /// </summary>
    /// <param name="database">The database to connect to.</param>
    /// <param name="token">The token used to cancel the request.</param>
    /// <returns>A <see cref="IDbConnection"/> instance to the specified database.</returns>
    /// <remarks>
    /// This method will attempt to create/migrate the database if the registered source path does not exist
    /// on disc. Ensure proper migration before calling if that is what is needed. Also note that this requires a data source
    /// path is registered to the manager first.
    /// </remarks>
    Task<IDbConnection> Connect(Database database, CancellationToken token);

    /// <summary>
    /// Migrates the database to the specified version.
    /// </summary>
    /// <param name="database">The database object to be migrated.</param>
    /// <param name="version">The version to migrate the database to (optional). The default value is 0 (latest).</param>
    /// <returns>The result of the migration operation.</returns>
    Result Migrate(Database database, long version = 0);
}