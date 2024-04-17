using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record DeleteRun(Guid RunId) : IDbCommand<Result>;

[UsedImplicitly]
internal class DeleteRunHandler(IConnectionManager manager) : IRequestHandler<DeleteRun, Result>
{
    private const string DeleteRun = "DELETE FROM Run WHERE RunId = @RunId";

    public async Task<Result> Handle(DeleteRun request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(Database.Project, cancellationToken);
        var result = await connection.ExecuteAsync(DeleteRun, new {request.RunId});
        return Result.OkIf(result == 1, $"Run not found: {request.RunId}");
    }
}