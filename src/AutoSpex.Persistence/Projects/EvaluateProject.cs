using System.Data.SQLite;
using System.Reflection;
using AutoSpex.Engine;
using Dapper;
using FluentMigrator;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

/// <summary>
/// This request will check the latest version for the project migrations and compare that to the provided project's
/// version stored in the local database file. from these version it will determine a <see cref="ProjectAction"/>
/// which will indicate what course of action the application must take in order to launch the project.
/// </summary>
[PublicAPI]
public record EvaluateProject(Project Project) : IDbQuery<Result<ProjectAction>>;

[UsedImplicitly]
internal class EvaluateProjectHandler : IRequestHandler<EvaluateProject, Result<ProjectAction>>
{
    public async Task<Result<ProjectAction>> Handle(EvaluateProject request, CancellationToken cancellationToken)
    {
        var connectionString = request.Project.ConnectionString;

        var targetVersion = await GetTargetVersion(connectionString);
        var currentVersion = GetCurrentVersion();

        var action = ProjectAction.DetermineAction(currentVersion, targetVersion);

        return Result.Ok(action);
    }
    
    /// <summary>
    /// Uses reflection to find the <see cref="MigrationIdAttribute"/> with the highest version number and returns
    /// the version to be used for comparison to the target database.
    /// </summary>
    private long GetCurrentVersion()
    {
        var attribute = GetType().Assembly.GetTypes()
                   .Where(t => typeof(IMigration).IsAssignableFrom(t))
                   .Where(t => t.CustomAttributes.Any(d => d.AttributeType == typeof(MigrationIdAttribute)) &&
                               t.CustomAttributes.Any(d => d.AttributeType == typeof(TagsAttribute)) &&
                               t.GetCustomAttribute<TagsAttribute>()!.TagNames.Contains("Project")
                   )
                   .Select(t => t.GetCustomAttribute<MigrationIdAttribute>())
                   .MaxBy(a => a!.Version)
               ?? throw new InvalidOperationException("Could not find the max version for the project database.");

        return attribute.Version;
    }

    /// <summary>
    /// This method fetches the maximum database version by executing a SQL query on the main.VersionInfo table of
    /// SQLite database.
    /// </summary>
    private static async Task<long> GetTargetVersion(string connectionString)
    {
        await using var connection = new SQLiteConnection(connectionString);
        await connection.OpenAsync();
        return await connection.QuerySingleAsync<long>("SELECT MAX(Version) FROM main.VersionInfo");
    }
}