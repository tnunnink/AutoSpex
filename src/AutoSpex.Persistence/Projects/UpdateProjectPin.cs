using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record UpdateProjectPin(Project Project) : IDbCommand<Result>;

[UsedImplicitly]
internal class UpdateProjectPinHandler(IConnectionManager manager) : IRequestHandler<UpdateProjectPin, Result>
{
    private const string SetPin = "UPDATE Project SET Pinned = @Pinned WHERE Path = @Path";

    public async Task<Result> Handle(UpdateProjectPin request, CancellationToken cancellationToken)
    {
        var project = request.Project;
        using var connection = await manager.Connect(Database.App, cancellationToken);
        var result = await connection.ExecuteAsync(SetPin, new { Path = project.Path.LocalPath, project.Pinned });
        return Result.OkIf(result == 1, $"Project Not Found: {project.Path}");
    }
}