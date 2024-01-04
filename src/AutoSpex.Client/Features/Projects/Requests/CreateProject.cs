using System;
using System.Threading;
using System.Threading.Tasks;
using AutoSpex.Client.Behaviors;
using AutoSpex.Client.Features.Projects.Services;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using Avalonia.Controls.Notifications;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Client.Features.Projects;

[PublicAPI]
public record CreateProjectRequest(Project Project) : INotifiableRequest<Result>
{
    public Notification? BuildNotification(Result result)
    {
        if (result.IsFailed)
        {
            return new Notification("Project Creation Error", 
                $"Failed to new create project {Project.Name}. See notification for further details.",
                NotificationType.Error);
        }

        return default;
    }
}

[UsedImplicitly]
public class CreateProjectHandler : IRequestHandler<CreateProjectRequest, Result>
{
    private const string Upsert = "INSERT INTO Project(Path, OpenedOn) VALUES(@Path, @OpenedOn)" +
                                  "ON CONFLICT DO UPDATE SET OpenedOn = @OpenedOn";

    private readonly AppDatabase _database;
    private readonly IProjectMigrator _migrator;

    public CreateProjectHandler(AppDatabase database, IProjectMigrator migrator)
    {
        _database = database;
        _migrator = migrator;
    }

    public async Task<Result> Handle(CreateProjectRequest request, CancellationToken cancellationToken)
    {
        var project = request.Project;
        
        //This will create and migrate the project.
        var migration = await _migrator.Migrate(project.Uri);
        if (migration.IsFailed)
        {
            return migration.WithError("Failed to create project due to migration result.");
        }

        project.OpenedOn = DateTime.Now;
        
        using var connection = await _database.Connect(cancellationToken);
        await connection.ExecuteAsync(Upsert, new { Path = project.Uri.LocalPath, project.OpenedOn });
        
        await Settings.App.SaveAsync(s => s.OpenProject = project.Uri.LocalPath);
        
        return Result.Ok();
    }
}