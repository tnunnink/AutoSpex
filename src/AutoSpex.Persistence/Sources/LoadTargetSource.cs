using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record LoadTargetSource : IDbQuery<Result<Source>>;

[UsedImplicitly]
internal class LoadTargetSourceHandler(IConnectionManager manager) : IRequestHandler<LoadTargetSource, Result<Source>>
{
    private const string GetSource =
        """
        SELECT SourceId, Name, IsTarget, TargetName, TargetType, ExportedBy, ExportedOn, Description, Content 
        FROM Source WHERE IsTarget = 1
        """;

    private const string GetSuppressions = "SELECT NodeId, Reason FROM Suppression WHERE SourceId = @SourceId";

    private const string GetOverrides = "SELECT Config FROM Override WHERE SourceId = @SourceId";

    public async Task<Result<Source>> Handle(LoadTargetSource request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);

        var source = await connection.QuerySingleOrDefaultAsync<Source>(GetSource);
        if (source is null)
            return Result.Fail("No source is currently targetd.");

        var suppressions = await connection.QueryAsync<Suppression>(GetSuppressions, new { source.SourceId });
        foreach (var suppression in suppressions)
            source.AddSuppression(suppression);

        var specs = await connection.QueryAsync<Spec>(GetOverrides, new { source.SourceId });
        foreach (var spec in specs)
            source.AddOverride(spec);

        return Result.Ok(source);
    }
}