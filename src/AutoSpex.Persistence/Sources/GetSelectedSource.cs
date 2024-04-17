using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record GetSelectedSource : IDbQuery<Result<Source?>>;

[UsedImplicitly]
internal class GetSelectedSourceHandler(IConnectionManager manager)
    : IRequestHandler<GetSelectedSource, Result<Source?>>
{
    private const string Query =
        "SELECT SourceId, IsSelected, Name, Documentation, TargetType, TargetName, ExportedOn, ExportedBy, Content " +
        "FROM Source WHERE IsSelected = 1";

    public async Task<Result<Source?>> Handle(GetSelectedSource request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(Database.Project, cancellationToken);
        var source = await connection.QuerySingleOrDefaultAsync<Source>(Query);
        return Result.Ok(source);
    }
}