using System.Threading;
using System.Threading.Tasks;
using ActiproSoftware.UI.Avalonia.Controls;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Client.Features;

[PublicAPI]
public record LaunchProjectRequest(Project Project) : IRequest<Result>
{
    /*public Notification? BuildNotification(Result<Project> result)
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
    }*/
}

[UsedImplicitly]
public class LaunchProjectHandler(IMediator mediator) : IRequestHandler<LaunchProjectRequest, Result>
{
    public async Task<Result> Handle(LaunchProjectRequest request, CancellationToken cancellationToken)
    {
        var project = request.Project;

        var action = await mediator.Send(new EvaluateProject(project), cancellationToken);
        if (action.IsFailed)
        {
            return Result.Fail(
                        $"Failed to evaluate the current version of the project '{project.Name}'. " +
                        $"Make sure this is a valid Spex project file and that the file exists locally.")
                    .WithErrors(action.Errors);
        }

        if (action.Value == ProjectAction.MigrationRequired)
        {
            var migrationResult = await HandleMigrationRequired(project, cancellationToken);
            if (migrationResult.IsFailed) return migrationResult;
            await mediator.Send(new LaunchProjectRequest(project), cancellationToken);
        }
        
        if (action.Value == ProjectAction.UpdateRequired)
        {
            return await HandleUpdateRequired(project, cancellationToken);
        }
        
        if (action.Value == ProjectAction.UpdateSuggested)
        {
            //todo handle just send notification or something. we will want to prompt them to update.
        }

        return await mediator.Send(new OpenProject(project), cancellationToken);
    }

    private async Task<Result> HandleMigrationRequired(Project project, CancellationToken token)
    {
        var result = await UserPromptBuilder.Configure()
            .WithHeaderContent("Migrate the project?")
            .WithContent("The specified project requires migration. Do you want to migrate the project?")
            .WithStandardButtons(MessageBoxButtons.YesNo)
            .WithStatusImage(MessageBoxImage.Question)
            .WithCheckBoxContent("_Always migrate projects")
            .Show();

        if (result != MessageBoxResult.Yes)
        {
            return Result.Fail(
                "Can not open project due to migration requirements. User has selected not to migrate.");
        }

        return await mediator.Send(new MigrateProject(project), token);
    }
    
    private async Task<Result> HandleUpdateRequired(Project project, CancellationToken token)
    {
        var result = await UserPromptBuilder.Configure()
            .WithHeaderContent("Migrate the project?")
            .WithContent("The specified project requires migration. Do you want to migrate the project?")
            .WithStandardButtons(MessageBoxButtons.YesNo)
            .WithStatusImage(MessageBoxImage.Question)
            .WithCheckBoxContent("_Always migrate projects")
            .Show();

        if (result != MessageBoxResult.Yes)
        {
            return Result.Fail(
                "Can not open project due to migration requirements. User has selected not to migrate.");
        }

        return await mediator.Send(new MigrateProject(project), token);
    }
}