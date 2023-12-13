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
    private readonly ISettingsManager _settings;
    private readonly IDataStoreProvider _dataStore;

    public CreateProjectHandler(IDataStoreProvider dataStore, IProjectMigrator migrator, ISettingsManager settings)
    {
        _dataStore = dataStore;
        _migrator = migrator;
        _settings = settings;
    }

    public async Task<Result<Project>> Handle(CreateProjectRequest request, CancellationToken cancellationToken)
    {
        //This will create and migrate the project.
        var migration = await _migrator.Migrate(request.Path);
        if (migration.IsFailed)
        {
            return migration.ToResult<Project>()
                .WithError("Failed to create project due to migration result.");
        }

        var project = new Project(request.Path)
        {
            OpenedOn = DateTime.UtcNow
        };
        
        using var connection = await _dataStore.ConnectTo(StoreType.Application, cancellationToken);
        await connection.ExecuteAsync(Upsert, new { Path = project.Uri.LocalPath, project.OpenedOn });
        
        _settings.Set(Setting.OpenProjectConnection, project.ConnectionString);
        _settings.Set(Setting.OpenProjectPath, project.Uri.LocalPath);
        await _settings.Save();
        
        return Result.Ok(project);
    }
}