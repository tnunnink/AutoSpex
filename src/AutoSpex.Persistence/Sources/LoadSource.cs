using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;
using Action = AutoSpex.Engine.Action;

namespace AutoSpex.Persistence;

[PublicAPI]
public record LoadSource(Guid SourceId) : IDbQuery<Result<Source>>;

[UsedImplicitly]
internal class LoadSourceHandler(IConnectionManager manager) : IRequestHandler<LoadSource, Result<Source>>
{
    private const string GetSource =
        """
        SELECT SourceId, Name, IsTarget, TargetName, TargetType, ExportedBy, ExportedOn, Description, Content 
        FROM Source WHERE SourceId = @SourceId
        """;

    private const string GetActions = "SELECT NodeId, Type, Reason, Config FROM Action WHERE SourceId = @SourceId";

    public async Task<Result<Source>> Handle(LoadSource request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);

        var source = await connection.QuerySingleOrDefaultAsync<Source>(GetSource, new { request.SourceId });
        if (source is null) return Result.Fail($"Source not found: {request.SourceId}");

        var rules = (await connection.QueryAsync<Action>(GetActions, new { source.SourceId })).ToList();
        rules.ForEach(source.AddRule);

        return Result.Ok(source);
    }
}