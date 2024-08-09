using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record DeleteEnvironments(IEnumerable<Guid> Ids) : IDbCommand<Result>;

[UsedImplicitly]
internal class DeleteEnvironmentsHandler(IConnectionManager manager) : IRequestHandler<DeleteEnvironments, Result>
{
    private const string DeleteEnvironments = "DELETE FROM Environment WHERE EnvironmentId IN @Ids";

    public async Task<Result> Handle(DeleteEnvironments request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        var ids = request.Ids.Select(n => n.ToString()).ToList();
        await connection.ExecuteAsync(DeleteEnvironments, new { Ids = ids });
        return Result.Ok();
    }
}