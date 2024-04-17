using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record RenameRun(Run Run) : IDbCommand<Result>;

[UsedImplicitly]
internal class RenameRunHandler(IConnectionManager manager) : IRequestHandler<RenameRun, Result>
{
    private const string Command = "UPDATE Run SET Name = @Name WHERE RunId = @RunId";
    
    public async Task<Result> Handle(RenameRun request, CancellationToken cancellationToken)
    {
        var connection = await manager.Connect(Database.Project, cancellationToken);
        var updated = await connection.ExecuteAsync(Command, request.Run);
        return Result.OkIf(updated == 1, $"Run not found: '{request.Run.RunId}'");
    }
}