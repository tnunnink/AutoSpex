using System.Text.Json;
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

    private const string GetOverrides =
        """
        SELECT V.*, O.Value 
        FROM Override O 
            JOIN Variable V on O.VariableId = V.VariableId 
        WHERE SourceId = @SourceId
        """;

    public async Task<Result<Source>> Handle(LoadSource request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);

        var source = await connection.QuerySingleOrDefaultAsync<Source>(GetSource, new { request.SourceId });

        if (source is null)
            return Result.Fail($"Source not found: {request.SourceId}");

        var options = new JsonSerializerOptions { Converters = { new JsonObjectConverter() } };

        await connection.QueryAsync<Variable, string, Variable>(GetOverrides,
            (variable, value) =>
            {
                var typed = JsonSerializer.Deserialize<object>(value, options);
                variable.Value = typed;
                source.AddOverride(variable);
                return variable;
            },
            splitOn: "Value",
            param: new { source.SourceId }
        );

        return Result.Ok(source);
    }
}