using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ActiproSoftware.UI.Avalonia.Controls;
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
public record LaunchProjectRequest(Uri Path) : INotifiableRequest<Result<Project>>
{
    public Notification? BuildNotification(Result<Project> result)
    {
        if (result.IsFailed)
        {
            return new Notification("Project Migration Failed",
                result.Errors.First().ToString(),
                NotificationType.Error);
        }

        if (result.IsSuccess)
        {
            return new Notification("Project Migration Successful",
                result.Successes.FirstOrDefault()?.ToString() ?? "Nothing specified",
                NotificationType.Success);
        }

        return default;
    }
}

[UsedImplicitly]
public class LaunchProjectHandler : IRequestHandler<LaunchProjectRequest, Result<Project>>
{
    private const string Upsert = "INSERT INTO Project(Path, OpenedOn) VALUES(@Path, @OpenedOn)" +
                                  "ON CONFLICT DO UPDATE SET OpenedOn = excluded.OpenedOn";
    
    private readonly IDataStoreProvider _dataStore;
    private readonly IProjectMigrator _migrator;
    private readonly ISettingsManager _settings;

    public LaunchProjectHandler(IDataStoreProvider dataStore, IProjectMigrator migrator, ISettingsManager settings)
    {
        _dataStore = dataStore;
        _migrator = migrator;
        _settings = settings;
    }

    public async Task<Result<Project>> Handle(LaunchProjectRequest request, CancellationToken cancellationToken)
    {
        var action = _migrator.Evaluate(request.Path);

        if (action.IsFailed)
        {
            return Result.Fail("Project Launch Failed")
                .WithError("Could not determine migration action.")
                .WithErrors(action.Errors);
        }

        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault we dont handle anything else here.
        switch (action.Value)
        {
            case ProjectAction.MigrationRequired:
            {
                //todo check settings to see if user say to always migrate regardless.
                //todo Add callback to set the local client settings to ignore migration prompt.

                var result = await UserPromptBuilder.Configure()
                    .WithHeaderContent("Migrate the project?")
                    .WithContent("The specified project requires migration. Do you want to migrate the project?")
                    .WithStandardButtons(MessageBoxButtons.YesNo)
                    .WithStatusImage(MessageBoxImage.Question)
                    .WithCheckBoxContent("_Always migrate projects")
                    .Show();

                if (result != MessageBoxResult.Yes)
                {
                    return Result.Fail<Project>(
                        "Can not open project due to migration requirements. User has selected not to migrate.");
                }

                await _migrator.Migrate(request.Path);
                break;
            }
            case ProjectAction.UpdateRequired:
            {
                var result = await UserPromptBuilder.Configure()
                    .WithHeaderContent("Application Update Required")
                    .WithContent("The selected project's version is higher than the application can support. To launch this project, you must update the application. Do you want to update the application now?")
                    .WithStandardButtons(MessageBoxButtons.YesNo)
                    .WithStatusImage(MessageBoxImage.Error)
                    .Show();

                if (result != MessageBoxResult.Yes)
                {
                    return Result.Fail<Project>(
                        "Can not open project due to migration requirements. User has selected not to migrate or update.");
                }

                //todo insert method of application update here via service or whatever
                break;
            }
            case ProjectAction.UpdateSuggested:
                break;
        }

        var project = new Project(request.Path)
        {
            OpenedOn = DateTime.UtcNow
        };

        _settings.Set(Setting.OpenProjectConnection, project.ConnectionString);
        _settings.Set(Setting.OpenProjectPath, project.Uri.LocalPath);
        await _settings.Save();

        using var connection = await _dataStore.ConnectTo(StoreType.Application, cancellationToken);
        await connection.ExecuteAsync(Upsert, new {Path = project.Uri.LocalPath, project.OpenedOn});
        return Result.Ok(project);
    }
}