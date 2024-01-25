using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

/// <summary>
/// Gets the number of projects stored in the projects table of the local application database.
/// </summary>
[PublicAPI]
public record GetProjectCount : IDbQuery<Result<int>>;

[UsedImplicitly]
internal class GetProjectCountHandler(IConnectionManager manager) : IRequestHandler<GetProjectCount, Result<int>>
{
    public async Task<Result<int>> Handle(GetProjectCount request, CancellationToken cancellationToken)
    {
        var connection = await manager.Connect(Database.App, cancellationToken);
        var result = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Project", cancellationToken);
        return Result.Ok(result);
    }
}