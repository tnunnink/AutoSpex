using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

/// <summary>
/// This command will update the local application database project OpenedOn date time and register the provided
/// project to the <see cref="IConnectionManager"/>. This will allow all subsequent project db commands
/// to be directed towards the supplied project path.
/// </summary>
/// <param name="Project">The <see cref="Engine.Project"/> to open.</param>
[PublicAPI]
public record OpenProject(Project Project) : IDbCommand<Result>;

[UsedImplicitly]
internal class OpenProjectHandler(IConnectionManager manager) : IRequestHandler<OpenProject, Result>
{
    private const string Update = "INSERT INTO Project(Path, OpenedOn) VALUES (@Path, @OpenedOn)" +
                                  "ON CONFLICT DO UPDATE SET OpenedOn = @OpenedOn";

    public async Task<Result> Handle(OpenProject request, CancellationToken cancellationToken)
    {
        var project = request.Project;
        project.OpenedOn = DateTime.Now;
        
        var connection = await manager.Connect(Database.App, cancellationToken);
        await connection.ExecuteAsync(Update, new {Path = project.Uri.LocalPath, project.OpenedOn, project.Summary});
        
        manager.Register(Database.Project, project.Uri.LocalPath);
        
        return Result.Ok();
    }
}