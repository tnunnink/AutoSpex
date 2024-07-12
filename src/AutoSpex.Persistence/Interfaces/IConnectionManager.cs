using System.Data;
using FluentResults;

namespace AutoSpex.Persistence;

/// <summary>
/// An interface for managing the database connection for this layer. This interface allows connection and migration
/// of the application database. External classes don't really need to worry about this because it is only required
/// for the persistence layer requests.
/// </summary>
public interface IConnectionManager
{
    /// <summary>
    /// Opens a connection to the application database and returns the new <see cref="IDbConnection"/> instance.
    /// </summary>
    /// <param name="token">The token used to cancel the request.</param>
    /// <returns>A <see cref="IDbConnection"/> instance to the specified database.</returns>
    /// <remarks>
    /// This method will attempt to create/migrate the database if the registered source path does not exist
    /// on disc. Ensure proper migration before calling if that is what is needed.
    /// </remarks>
    Task<IDbConnection> Connect(CancellationToken token);
}