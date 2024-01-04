using System;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoSpex.Client.Migrations;
using Dapper;
using FluentMigrator;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using FluentResults;
using JetBrains.Annotations;
using Lamar;
using Microsoft.Extensions.DependencyInjection;

namespace AutoSpex.Client.Features.Projects.Services;

[UsedImplicitly]
public class ProjectMigrator : IProjectMigrator
{
    public Task<Result> Migrate(Uri path)
    {
        return Task.Run(() =>
        {
            var connectionString = BuildConnectionString(path);
            var container = BuildContainer(connectionString);
            var migrator = container.GetInstance<IMigrationRunner>();

            var backup = File.Exists(path.AbsolutePath) ? Backup(path.AbsolutePath) : default;
            if (backup is not null && backup.IsFailed)
            {
                return Result
                    .Fail("Project backup failed. Please ensure all connections to te project are closed.")
                    .WithErrors(backup.Errors);
            }

            var result = Result.Try(() =>
                {
                    migrator.MigrateUp();
                    backup?.Value.Delete();
                },
                e =>
                {
                    var restore = backup is not null ? RestoreBackup(backup.Value, path.AbsolutePath) : default;
                    if (restore is not null && restore.IsFailed)
                    {
                        return new Error(
                                $"Project restoration failed. Backup file preserved at '{backup?.Value.FullName}'.")
                            .CausedBy(e);
                    }
                    
                    backup?.Value.Delete();
                    return new Error("Project migration failed.").CausedBy(e);
                });

            container.Dispose();
            return result;
        });
    }

    private static Result<FileInfo> RestoreBackup(FileInfo backup, string path)
    {
        return Result.Try(() => backup.CopyTo(path));
    }

    /// <summary>
    /// Evaluates the necessary action to align the currently running application to the version of the database at the given URI.
    /// </summary>
    /// <param name="uri">The URI where the SQLite database is located.</param>
    /// <returns>
    /// Returns a Result of ProjectLaunchAction that specifies what action is required:
    /// - NoAction, if versions are equal or only their patch versions differ.
    /// - MigrationRequired, if the application has a higher major or minor version.
    /// - UpdateRequired, if the application has a lower major version.
    /// - UpdateSuggested, if application and project have the same major version, but the application has a lower minor version.
    /// </returns>
    public Result<ProjectAction> Evaluate(Uri uri)
    {
        var connectionString = BuildConnectionString(uri);

        var dbVersion = GetDbVersion(connectionString);
        var appVersion = GetAppVersion();

        return Result.Ok(appVersion.DetermineActionTo(dbVersion));
    }

    /// <summary>
    /// Configures and returns an instance of FluentMigrator's IMigrationRunner.
    /// </summary>
    /// <param name="connectionString">The connection string to the SQLite database.</param>
    /// <returns>
    /// An IMigrationRunner instance configured to use the provided connectionString,
    /// and to scan the current assembly for migrations tagged with "Project".
    /// </returns>
    private IContainer BuildContainer(string connectionString)
    {
        var registry = new ServiceRegistry();

        registry.AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddSQLite()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(GetType().Assembly).For.Migrations())
            .Configure<RunnerOptions>(o => { o.Tags = new[] {"Project"}; })
            /*.AddLogging(lb => lb.AddSerilog())*/;

        return new Lamar.Container(registry);
    }

    /// <summary>
    /// Retrieves the MigrationIdAttribute instance with the maximum version from all migrations tagged as "Project".
    /// </summary>
    /// <returns>
    /// The MigrationIdAttribute instance with the highest version number found among migrations tagged as "Project".
    /// </returns>
    /// <exception cref="System.InvalidOperationException">
    /// Thrown when no migration with a MigrationIdAttribute and a "Project" tag can be found in any types in the current assembly.
    /// </exception>
    private MigrationIdAttribute GetAppVersion()
    {
        return GetType().Assembly.GetTypes()
                   .Where(t => typeof(IMigration).IsAssignableFrom(t))
                   .Where(t => t.CustomAttributes.Any(d => d.AttributeType == typeof(MigrationIdAttribute)) &&
                               t.CustomAttributes.Any(d => d.AttributeType == typeof(TagsAttribute)) &&
                               t.GetCustomAttribute<TagsAttribute>()!.TagNames.Contains("Project")
                   )
                   .Select(t => t.GetCustomAttribute<MigrationIdAttribute>())
                   .MaxBy(a => a!.Version)
               ?? throw new InvalidOperationException("Could not find the max version for the project database.");
    }


    /// <summary>
    /// This method fetches the maximum database version by executing a SQL query on the main.VersionInfo table of
    /// SQLite database.
    /// </summary>
    /// <param name="connectionString">The connection string for the SQLite database.</param>
    /// <returns>
    /// Returns a <see cref="MigrationIdAttribute"/> object representing the highest database version
    /// obtained from the main.VersionInfo table. The <see cref="MigrationIdAttribute"/> is a user-defined 
    /// attribute that is used to annotate database migration classes with a unique identifier - in this case, 
    /// representing the DB version number. <see cref="MigrationIdAttribute.FromVersion"/> is a method that is 
    /// used for constructing a new instance of <see cref="MigrationIdAttribute"/> from a version number.
    /// </returns>
    /// <remarks>
    /// This method assumes that there exists a `VersionInfo` table on the `main` schema of the SQLite database 
    /// and this table contains a `Version` column of type `long`.
    /// </remarks>
    private static MigrationIdAttribute GetDbVersion(string connectionString)
    {
        using var connection = new SQLiteConnection(connectionString);
        connection.Open();
        var version = connection.QuerySingle<long>("SELECT MAX(Version) FROM main.VersionInfo");
        connection.Close();
        return MigrationIdAttribute.FromVersion(version);
    }


    /// <summary>
    /// Creates a backup of a given file.
    /// </summary>
    /// <param name="path">The path to the file that needs to be backed up.</param>
    /// <returns>
    /// If successful, returns a Result with the FileInfo of the backup file.
    /// If the specified file does not exist, returns a failure Result with a corresponding message.
    /// If a file operation failure occurs, returns a failure Result with an error describing the failure.
    /// The error includes the original exception as its cause.
    /// </returns>
    private static Result<FileInfo> Backup(string path)
    {
        var info = new FileInfo(path);

        if (!info.Exists)
            return Result.Fail("The path to the project does not exist");

        var directoryName = info.DirectoryName ?? string.Empty;
        var backupName = $"{info.Name}.BACKUP{DateTime.Now.Ticks}{info.Extension}";
        var backupPath = Path.Combine(directoryName, backupName);

        return Result.Try(() => info.CopyTo(backupPath),
            e => new Error("Failed to copy the current project to a backup file.").CausedBy(e));
    }

    /// <summary>
    /// Constructs a connection string for a SQLite database located at the provided path.
    /// </summary>
    /// <param name="path">The URI representing the path to the SQLite database file.</param>
    /// <returns>
    /// Returns a string that represents the connection string to the SQLite database. The connection string 
    /// includes the absolute path to the database file as the data source.
    /// </returns>
    /// <remarks>
    /// The built connection string doesn't include any parameters other than the data source. If additional 
    /// parameters are required (such as Version, Pooling, etc.), they need to be added to the SQLiteConnectionStringBuilder.
    /// </remarks>
    private static string BuildConnectionString(Uri path)
    {
        var builder = new SQLiteConnectionStringBuilder
        {
            DataSource = path.AbsolutePath,
            Pooling = false
        };

        return builder.ConnectionString;
    }
}