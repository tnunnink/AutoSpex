using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record GetTargetSource : IRequest<Result<Source>>;

[UsedImplicitly]
internal class GetTargetSourceHandler(IConnectionManager manager) : IRequestHandler<GetTargetSource, Result<Source>>
{
    private const string GetTarget =
        """
        SELECT SourceId, Name, IsTarget, TargetName, TargetType, ExportedBy, ExportedOn, Description 
        FROM Source WHERE IsTarget = 1
        """;

    public async Task<Result<Source>> Handle(GetTargetSource request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        var source = await connection.QuerySingleOrDefaultAsync<Source>(GetTarget);
        return source is not null ? Result.Ok(source) : Result.Fail("No target source selected.");
    }
}