using System.Text.Json;
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

    private const string GetOverrides =
        "SELECT V.*, O.Value FROM Override O JOIN Variable V on O.VariableId = V.VariableId WHERE SourceId = @SourceId";

    public async Task<Result<Source>> Handle(LoadTargetSource request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);

        var source = await connection.QuerySingleOrDefaultAsync<Source>(GetSource);
        if (source is null)
            return Result.Fail("No source is currently targetd.");

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