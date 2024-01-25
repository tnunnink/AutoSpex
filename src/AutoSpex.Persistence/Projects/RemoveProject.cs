using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

/// <summary>
/// Simply removes the provided project from the local application database without deleting the file from disc.
/// </summary>
/// <param name="Project">The project to remove.</param>
[PublicAPI]
public record RemoveProject(Project Project) : IDbCommand<Result>;

[UsedImplicitly]
internal class RemoveProjectHandler(IConnectionManager manager) : IRequestHandler<RemoveProject, Result>
{
    private const string Command = "DELETE FROM Project WHERE Path = @Path";

    public async Task<Result> Handle(RemoveProject request, CancellationToken cancellationToken)
    {
        var project = request.Project;
        var connection = await manager.Connect(Database.App, cancellationToken);
        var affected = await connection.ExecuteAsync(Command, new {Path = project.Uri.LocalPath});
        return Result.Ok().WithSuccess($"Removed {affected} project(s) from application store.");
    }
}