using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Pages;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Client.Services;

/// <summary>
/// A service for managing the use of project files.
/// Will load all known projects, open projects, hold reference to open project, migrate projects,
/// monitor project for changes, as well as dispose projects when closing.
/// </summary>
[UsedImplicitly]
[PublicAPI]
public sealed class Manager(IMediator mediator, Navigator navigator, Prompter prompter) : IRecipient<ProjectsRequest>
{
    private ProjectPageModel? _openPage;

    public async Task<IEnumerable<ProjectObserver>> GetAllProjects()
    {
        var result = await mediator.Send(new ListProjects());
        if (result.IsFailed) return Enumerable.Empty<ProjectObserver>();
        return result.Value.Select(v => new ProjectObserver(v));
    }
    
    public async Task<IEnumerable<ProjectObserver>> GetProjects()
    {
        var result = await mediator.Send(new ListProjects());
        if (result.IsFailed) return Enumerable.Empty<ProjectObserver>();
        return result.Value.Select(v => new ProjectObserver(v));
    }

    public async Task OpenProject(ProjectObserver project)
    {
        if (_openPage is not null && _openPage.Route.Equals(project.Uri.LocalPath))
        {
            navigator.Close(_openPage);
            _openPage = null;
        }
        
        var result = await OpenProject(project.Model, CancellationToken.None);
        if (result.IsFailed) return;
        
        var page = await navigator.Navigate(() => new ProjectPageModel(project));
        _openPage = page;
    }

    public void Receive(Observer<Project>.Created message)
    {
    }

    public void Receive(Observer<Project>.Deleted message)
    {
    }
    
    private async Task<Result> OpenProject(Project project, CancellationToken cancellationToken)
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
        var result = await prompter.PromptMigrate(project.Name);

        if (result is false)
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

    public void Receive(ProjectsRequest message) => message.Reply(GetProjects());
}

public class ProjectsRequest : AsyncRequestMessage<IEnumerable<ProjectObserver>>;