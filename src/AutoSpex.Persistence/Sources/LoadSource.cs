using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

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

    private const string GetSuppressions = "SELECT NodeId, Reason FROM Suppression WHERE SourceId = @SourceId";

    private const string GetOverrides = "SELECT Config FROM Override WHERE SourceId = @SourceId";

    public async Task<Result<Source>> Handle(LoadSource request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);

        var source = await connection.QuerySingleOrDefaultAsync<Source>(GetSource, new { request.SourceId });
        if (source is null)
            return Result.Fail($"Source not found: {request.SourceId}");

        var suppressions = await connection.QueryAsync<Suppression>(GetSuppressions, new { request.SourceId });
        foreach (var suppression in suppressions)
            source.AddSuppression(suppression);

        var specs = await connection.QueryAsync<Spec>(GetOverrides, new { request.SourceId });
        foreach (var spec in specs)
            source.AddOverride(spec);

        return Result.Ok(source);
    }
}