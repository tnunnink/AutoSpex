using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record DeleteEnvironment(Guid EnvironmentId) : IDbCommand<Result>;

[UsedImplicitly]
internal class DeleteEnvironmentHandler(IConnectionManager manager) : IRequestHandler<DeleteEnvironment, Result>
{
    private const string DeleteEnvironment = "DELETE FROM Environment WHERE EnvironmentId = @EnvironmentId";

    public async Task<Result> Handle(DeleteEnvironment request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        var result = await connection.ExecuteAsync(DeleteEnvironment, new { request.EnvironmentId });
        return Result.Ok().WithSuccess($"Successfully deleted {result} environments");
    }
}