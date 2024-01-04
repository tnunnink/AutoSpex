using System.Threading;
using System.Threading.Tasks;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Client.Features.Projects;

[PublicAPI]
public record RemoveProjectRequest(Project Project) : IRequest<Result>;

[UsedImplicitly]
public class RemoveProjectHandler : IRequestHandler<RemoveProjectRequest, Result>
{
    private const string Command = "DELETE FROM Project WHERE Path = @Path";

    private readonly AppDatabase _database;

    public RemoveProjectHandler(AppDatabase database)
    {
        _database = database;
    }
    
    public async Task<Result> Handle(RemoveProjectRequest request, CancellationToken cancellationToken)
    {
        var project = request.Project;
        var connection = await _database.Connect(cancellationToken);
        var affected = await connection.ExecuteAsync(Command, new {Path = project.Uri.LocalPath});
        return Result.Ok().WithSuccess($"Removed {affected} project(s) from application store.");
    }
}