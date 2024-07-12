using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;
using Environment = AutoSpex.Engine.Environment;

namespace AutoSpex.Persistence;

[PublicAPI]
public record ListEnvironments : IDbQuery<Result<IEnumerable<Environment>>>;

[UsedImplicitly]
internal class ListEnvironmentsHandler(IConnectionManager manager)
    : IRequestHandler<ListEnvironments, Result<IEnumerable<Environment>>>
{
    private const string ListEnvironments = "SELECT EnvironmentId, Name, Comment, IsTarget FROM Environment";

    public async Task<Result<IEnumerable<Environment>>> Handle(ListEnvironments request,
        CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        var results = await connection.QueryAsync<Environment>(ListEnvironments);
        return Result.Ok(results);
    }
}