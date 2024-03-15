using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record DeleteRunner(Guid RunnerId) : IDbCommand<Result>;

[UsedImplicitly]
internal class DeleteRunnerHandler(IConnectionManager manager) : IRequestHandler<DeleteRunner, Result>
{
    private const string DeleteRunner = "DELETE FROM Runner WHERE RunnerId = @RunnerId";

    public async Task<Result> Handle(DeleteRunner request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(Database.Project, cancellationToken);
        var result = await connection.ExecuteAsync(DeleteRunner, new {request.RunnerId});
        return Result.OkIf(result == 1, $"Runner not found: {request.RunnerId}");
    }
}