using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record LoadSource(Guid SourceId) : IRequest<Result<Source>>;

[UsedImplicitly]
internal class LoadSourceHandler(IConnectionManager manager) : IRequestHandler<LoadSource, Result<Source>>
{
    private const string GetSource =
        """
        SELECT SourceId, Name, IsTarget, TargetName, TargetType, ExportedBy, ExportedOn, Description, Content 
        FROM Source WHERE SourceId = @SourceId
        """;

    public async Task<Result<Source>> Handle(LoadSource request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        var source = await connection.QuerySingleOrDefaultAsync<Source>(GetSource, new { request.SourceId });
        return source is null ? Result.Fail($"Source not found: {request.SourceId}") : Result.Ok(source);
    }
}