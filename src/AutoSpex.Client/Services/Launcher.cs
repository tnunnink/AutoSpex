using System;
using System.Threading;
using System.Threading.Tasks;
using ActiproSoftware.UI.Avalonia.Controls;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Client.Services;

[UsedImplicitly]
[PublicAPI]
// ReSharper disable once SuggestBaseTypeForParameterInConstructor
public sealed class Launcher(IMediator mediator)
{
    public async Task<Result> Launch(Project project, CancellationToken cancellationToken)
    {
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
            var migration = await HandleMigrationRequired(project, cancellationToken);
            if (migration.IsFailed) return migration;
            await mediator.Send(new OpenProject(project), cancellationToken);
        }
        
        if (action.Value == ProjectAction.UpdateRequired)
        {
            var update = await HandleUpdateRequired(project, cancellationToken);
        }
        
        if (action.Value == ProjectAction.UpdateSuggested)
        {
            //todo handle just send notification or something. we will want to prompt them to update.
        }

        return await mediator.Send(new OpenProject(project), cancellationToken);
    }

    private async Task<Result> HandleMigrationRequired(Project project, CancellationToken token)
    {
        var message =
            $"""
             The project {project.Name} requires migration to the latest schema in order to be compatible
             with this version of AutoSpex. Perform migration?
             """;
        
        var result = await UserPromptBuilder.Configure()
            .WithHeaderContent("Migrate Required")
            .WithContent(message)
            .WithStandardButtons(MessageBoxButtons.YesNo)
            .WithStatusImage(MessageBoxImage.Question)
            .WithFooterContent("")
            .Show();

        if (result != MessageBoxResult.Yes)
        {
            return Result.Fail(
                "Can not open project due to migration requirements. User has selected not to migrate.");
        }

        return await mediator.Send(new MigrateProject(project), token);
    }
    
    private Task<Result> HandleUpdateRequired(Project project, CancellationToken token)
    {
        throw new NotImplementedException();
    }
}