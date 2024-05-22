using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record GetSource(Guid SourceId) : IDbQuery<Result<Source>>;

[UsedImplicitly]
internal class GetSourceHandler(IConnectionManager manager) : IRequestHandler<GetSource, Result<Source>>
{
    private const string GetSource =
        """
        SELECT SourceId, Name, TargetType, TargetName, ExportedBy, ExportedOn, Content
        FROM Source S 
        JOIN Node N ON N.NodeId = S.SourceId
        WHERE SourceId = @SourceId
        """;

    public async Task<Result<Source>> Handle(GetSource request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(Database.Project, cancellationToken);
        var source = await connection.QuerySingleOrDefaultAsync<Source>(GetSource, new { request.SourceId });
        return source is not null ? Result.Ok(source) : Result.Fail<Source>($"Source not found: '{request.SourceId}'");
    }
}