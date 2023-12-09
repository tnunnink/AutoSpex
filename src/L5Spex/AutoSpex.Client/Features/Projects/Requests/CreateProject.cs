using System;
using System.Threading;
using System.Threading.Tasks;
using AutoSpex.Client.Behaviors;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using Avalonia.Controls.Notifications;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Client.Features.Projects;

[PublicAPI]
public record CreateProjectRequest(Uri Path) : INotifiableRequest<Result<Project>>
{
    public Notification? BuildNotification(Result<Project> result)
    {
        return new Notification("Test", "This is a test");
    }
}

[UsedImplicitly]
public class CreateProjectHandler : IRequestHandler<CreateProjectRequest, Result<Project>>
{
    private const string Upsert = "INSERT INTO Project(Path, OpenedOn) VALUES(@Path, @OpenedOn)" +
                                  "ON CONFLICT DO UPDATE SET OpenedOn = @OpenedOn";
    
    private readonly IProjectMigrator _migrator;
    private readonly IDataStoreProvider _dataStore;

    public CreateProjectHandler(IDataStoreProvider dataStore, IProjectMigrator migrator)
    {
        _dataStore = dataStore;
        _migrator = migrator;
    }

    public async Task<Result<Project>> Handle(CreateProjectRequest request, CancellationToken cancellationToken)
    {
        //This will create and migrate the project.
        var migration = await _migrator.Migrate(request.Path);
        if (migration.IsFailed)
        {
            return migration.ToResult<Project>();
        }

        var project = new Project(request.Path)
        {
            OpenedOn = DateTime.UtcNow
        };
        
        using var connection = await _dataStore.ConnectTo(StoreType.Application, cancellationToken);
        await connection.ExecuteAsync(Upsert, new { Path = project.Path.AbsolutePath, project.OpenedOn });
        
        App.Settings.Save(Setting.OpenProjectConnection, project.ConnectionString);
        App.Settings.Save(Setting.OpenProjectPath, project.Path.AbsolutePath);
        
        return Result.Ok(project);
    }
}