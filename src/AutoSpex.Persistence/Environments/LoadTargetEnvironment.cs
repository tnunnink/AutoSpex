using System.Text.Json;
using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;
using Environment = AutoSpex.Engine.Environment;

namespace AutoSpex.Persistence;

[PublicAPI]
public record LoadTargetEnvironment : IDbQuery<Result<Environment>>;

[UsedImplicitly]
internal class LoadTargetEnvironmentHandler(IConnectionManager manager)
    : IRequestHandler<LoadTargetEnvironment, Result<Environment>>
{
    private const string GetTargetEnvironment =
        "SELECT EnvironmentId, Name, Comment, IsTarget FROM Environment WHERE IsTarget = 1";

    private const string GetSources =
        "SELECT SourceId, Name, Uri FROM Source S WHERE EnvironmentId = @EnvironmentId";

    private const string GetOverrides =
        "SELECT V.*, O.Value FROM Override O JOIN Variable V on O.VariableId = V.VariableId WHERE SourceId = @SourceId";

    public async Task<Result<Environment>> Handle(LoadTargetEnvironment request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);

        var environment = await connection.QuerySingleOrDefaultAsync<Environment>(GetTargetEnvironment);
        if (environment is null)
            return Result.Fail("No environment is currently targetd.");
        
        var sources = await connection.QueryAsync<Source>(GetSources, new { environment.EnvironmentId });
        var options = new JsonSerializerOptions { Converters = { new JsonObjectConverter() } };
        
        foreach (var source in sources)
        {
            var overrides = await connection.QueryAsync<Variable, string, Variable>(GetOverrides,
                (variable, value) =>
                {
                    var typed = JsonSerializer.Deserialize<object>(value, options);
                    variable.Value = typed;
                    return variable;
                },
                splitOn: "Value",
                param: new { source.SourceId }
            );

            source.Overrides.AddRange(overrides);
            environment.Sources.Add(source);
        }

        return Result.Ok(environment);
    }
}