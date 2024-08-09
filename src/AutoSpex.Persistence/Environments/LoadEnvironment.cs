using System.Text.Json;
using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;
using Environment = AutoSpex.Engine.Environment;

namespace AutoSpex.Persistence;

[PublicAPI]
public record LoadEnvironment(Guid EnvironmentId) : IDbQuery<Result<Environment>>;

[UsedImplicitly]
internal class LoadEnvironmentHandler(IConnectionManager manager)
    : IRequestHandler<LoadEnvironment, Result<Environment>>
{
    private const string GetEnvironment =
        "SELECT EnvironmentId, Name, Comment, IsTarget FROM Environment WHERE EnvironmentId = @EnvironmentId";

    private const string GetSources =
        "SELECT SourceId, Name, Uri FROM Source S WHERE EnvironmentId = @EnvironmentId";

    private const string GetOverrides =
        "SELECT V.*, O.Value FROM Override O JOIN Variable V on O.VariableId = V.VariableId WHERE SourceId = @SourceId";

    public async Task<Result<Environment>> Handle(LoadEnvironment request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);

        var environment = await connection.QuerySingleOrDefaultAsync<Environment>(
            GetEnvironment,
            new { request.EnvironmentId }
        );

        if (environment is null) return Result.Fail($"Environment not found: {request.EnvironmentId}");

        var sources = await connection.QueryAsync<Source>(GetSources, new { request.EnvironmentId });
        foreach (var source in sources)
        {
            var overrides = await connection.QueryAsync<Variable, string, Variable>(GetOverrides,
                (variable, value) =>
                {
                    var options = new JsonSerializerOptions { Converters = { new JsonObjectConverter() } };
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