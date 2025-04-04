using System.Data;
using AutoSpex.Engine;

namespace AutoSpex.Persistence;

/// <summary>
/// An interface for managing the database connection for this layer.
/// </summary>
public interface IConnectionManager
{
    /// <summary>
    /// Opens a connection to the application database and returns the new <see cref="IDbConnection"/> instance.
    /// </summary>
    /// <param name="token">The token used to cancel the request.</param>
    /// <returns>A <see cref="IDbConnection"/> instance to the specified database.</returns>
    /// <remarks>
    /// This method will attempt to create and migrate the database if it does not exist on disc.
    /// </remarks>
    Task<IDbConnection> Connect(CancellationToken token);
}