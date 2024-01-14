using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record CreateProject(Project Project) : ICommand<Result>;

[UsedImplicitly]
internal class CreateProjectHandler(IConnectionManager manager)
    : IRequestHandler<CreateProject, Result>
{
    private const string Create = "INSERT INTO Project(Path, OpenedOn) VALUES(@Path, @OpenedOn)";

    public async Task<Result> Handle(CreateProject request, CancellationToken cancellationToken)
    {
        var project = request.Project;
        
        //Configures the project as the default for the manager.
        manager.Register(Database.Project, project.Uri.LocalPath);

        //This will create and migrate the project to the current version.
        var migration = manager.Migrate(Database.Project);
        if (migration.IsFailed)
        {
            return migration.WithError($"Failed to create project '{project.Name}' due to migration failure.");
        }

        project.OpenedOn = DateTime.Now;
        using var connection = await manager.Connect(Database.App, cancellationToken);
        await connection.ExecuteAsync(Create, new {Path = project.Uri.LocalPath, project.OpenedOn});
        
        return Result.Ok();
    }
}