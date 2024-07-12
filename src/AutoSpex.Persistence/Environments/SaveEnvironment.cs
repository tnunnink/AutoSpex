using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;
using Environment = AutoSpex.Engine.Environment;

namespace AutoSpex.Persistence;

/// <summary>
/// Updates the environment configuration in the database to that of the provided instance.
/// </summary>
/// <param name="Environment">The environment to save.</param>
[PublicAPI]
public record SaveEnvironment(Environment Environment) : IDbCommand<Result>;

[UsedImplicitly]
internal class SaveEnvironmentHandler(IConnectionManager manager) : IRequestHandler<SaveEnvironment, Result>
{
    private const string Exists = "SELECT COUNT() FROM Environment WHERE EnvironmentId = @EnvironmentId";

    private const string SaveEnvironment =
        "UPDATE Environment SET Comment = @Comment WHERE EnvironmentId = @EnvironmentId;";

    private const string DeleteSources =
        "DELETE FROM Source WHERE EnvironmentId = @EnvironmentId";

    private const string InsertSource =
        "INSERT INTO Source (SourceId, EnvironmentId, Name, Uri) VALUES (@SourceId, @EnvironmentId, @Name, @Uri)";

    private const string InsertOverride =
        "INSERT INTO Override (OverrideId, SourceId, VariableId, Value) VALUES (@OverrideId, @SourceId, @VariableId, @Value)";


    public async Task<Result> Handle(SaveEnvironment request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);

        var exists = await connection.QuerySingleAsync<int>(Exists, new { request.Environment.EnvironmentId });
        if (exists == 0) return Result.Fail($"Environment not found: {request.Environment.EnvironmentId}");

        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(SaveEnvironment,
            new { request.Environment.EnvironmentId, request.Environment.Comment },
            transaction
        );
        
        await connection.ExecuteAsync(DeleteSources, new { request.Environment.EnvironmentId }, transaction);

        foreach (var source in request.Environment.Sources)
        {
            await connection.ExecuteAsync(InsertSource,
                new { source.SourceId, request.Environment.EnvironmentId, source.Name, source.Uri },
                transaction
            );

            foreach (var variable in source.Overrides)
            {
                await connection.ExecuteAsync(InsertOverride,
                    new { OverrideId = Guid.NewGuid(), source.SourceId, variable.VariableId, variable.Value },
                    transaction
                );
            }
        }

        transaction.Commit();
        return Result.Ok();
    }
}