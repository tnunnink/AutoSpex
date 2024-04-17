using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using L5Sharp.Core;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record GetSourceContent(Guid SourceId) : IDbQuery<Result<L5X>>;

[UsedImplicitly]
internal class GetSourceContentHandler(IConnectionManager manager) : IRequestHandler<GetSourceContent, Result<L5X>>
{
    private const string GetContent = "SELECT Content FROM Source WHERE SourceId = @SourceId";

    public async Task<Result<L5X>> Handle(GetSourceContent request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(Database.Project, cancellationToken);
        var result = await connection.QuerySingleOrDefaultAsync<string>(GetContent, new {request.SourceId});

        if (result is null)
            return Result.Fail($"Source not found: {request.SourceId}");

        var xml = result.Decompress();
        var content = L5X.Parse(xml);
        return Result.Ok(content);
    }
}