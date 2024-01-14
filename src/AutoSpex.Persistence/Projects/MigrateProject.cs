using AutoSpex.Engine;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

/// <summary>
/// Performs a migration of the provided <see cref="Engine.Project"/>. This will first attempt to create a backup
/// file on disc in case the migration fails. If the migration succeeds the backup is deleted and the result is returned.
/// if the migration fails, we will attempt to restore the project with the backup that was created. If the backup
/// process failed, this command will abort. This should maintain the safety of the project file and prevent erroneous
/// migrations from accidentally deleting a project file. 
/// </summary>
/// <param name="Project">The <see cref="Engine.Project"/> to migrate.</param>
[PublicAPI]
public record MigrateProject(Project Project) : ICommand<Result>;

[UsedImplicitly]
internal class MigrateProjectHandler(IConnectionManager manager) : IRequestHandler<MigrateProject, Result>
{
    public Task<Result> Handle(MigrateProject request, CancellationToken cancellationToken)
    {
        return Task.Run(() =>
        {
            var project = request.Project;

            var backup = File.Exists(project.Uri.LocalPath) ? Backup(project.Uri.LocalPath) : default;
            if (backup is not null && backup.IsFailed)
            {
                var backupFailure = Result
                    .Fail("Project backup failed. Please ensure all connections to te project are closed.")
                    .WithErrors(backup.Errors);
                return backupFailure;
            }

            try
            {
                var result = manager.Migrate(Database.Project);
                backup?.Value.Delete();
                return result;
            }
            catch (Exception e)
            {
                var restore = backup is not null ? RestoreBackup(backup.Value, project.Uri.LocalPath) : default;
                if (restore is not null && restore.IsFailed)
                {
                    return new Error(
                            $"Project migration and restoration failed. Backup file preserved at '{backup?.Value.FullName}'.")
                        .CausedBy(e);
                }

                backup?.Value.Delete();
                return new Error($"Project migration failed due to error '{e.Message}'.").CausedBy(e);
            }
        }, cancellationToken);
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

    private static Result<FileInfo> RestoreBackup(FileInfo backup, string path)
    {
        return Result.Try(() => backup.CopyTo(path));
    }
}