using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record LoadTargetSource : IRequest<Result<Source>>;

[UsedImplicitly]
internal class LoadTargetSourceHandler(IConnectionManager manager) : IRequestHandler<LoadTargetSource, Result<Source>>
{
    private const string GetSource =
        """
        SELECT SourceId, Name, IsTarget, TargetName, TargetType, ExportedBy, ExportedOn, Description, Content 
        FROM Source WHERE IsTarget = 1
        """;

    public async Task<Result<Source>> Handle(LoadTargetSource request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        var source = await connection.QuerySingleOrDefaultAsync<Source>(GetSource);
        return source is null ? Result.Fail("No source is currently targetd.") : Result.Ok(source);
    }
}