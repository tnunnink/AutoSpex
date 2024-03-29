﻿using System;
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
    
    private readonly IProjectMigrator _migrator;
    private readonly ISettingsManager _settings;
    private readonly IDataStoreProvider _dataStore;

    public CreateProjectHandler(IDataStoreProvider dataStore, IProjectMigrator migrator, ISettingsManager settings)
    {
        _dataStore = dataStore;
        _migrator = migrator;
        _settings = settings;
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
        
        using var connection = await _dataStore.ConnectTo(StoreType.Application, cancellationToken);
        await connection.ExecuteAsync(Upsert, new { Path = project.Uri.LocalPath, project.OpenedOn });
        
        _settings.Set(Setting.OpenProjectConnection, project.ConnectionString);
        _settings.Set(Setting.OpenProjectPath, project.Uri.LocalPath);
        await _settings.Save();
        
        return Result.Ok();
    }
}