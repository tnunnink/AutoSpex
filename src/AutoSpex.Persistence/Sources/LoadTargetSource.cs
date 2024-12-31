using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;
using Action = AutoSpex.Engine.Action;

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

    private const string GetActions = "SELECT NodeId, Type, Reason, Config FROM Action WHERE SourceId = @SourceId";

    public async Task<Result<Source>> Handle(LoadTargetSource request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);

        var source = await connection.QuerySingleOrDefaultAsync<Source>(GetSource);
        if (source is null) return Result.Fail("No source is currently targetd.");

        var rules = (await connection.QueryAsync<Action>(GetActions, new { source.SourceId })).ToList();
        rules.ForEach(source.AddRule);

        return Result.Ok(source);
    }
}