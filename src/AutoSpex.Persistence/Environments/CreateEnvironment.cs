using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;
using Environment = AutoSpex.Engine.Environment;

namespace AutoSpex.Persistence;

/// <summary>
/// Inserts a new <see cref="Engine.Environment"/> in the database given the provided isntance.
/// </summary>
/// <param name="Environment">The environment to create.</param>
[PublicAPI]
public record CreateEnvironment(Environment Environment) : IDbCommand<Result>;

[UsedImplicitly]
internal class CreateEnvironmentHandler(IConnectionManager manager) : IRequestHandler<CreateEnvironment, Result>
{
    private const string InsertEnvironment =
        """
        INSERT INTO Environment (EnvironmentId, Name, Comment)
        VALUES (@EnvironmentId, @Name, @Comment);
        """;

    public async Task<Result> Handle(CreateEnvironment request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);

        var record = new
        {
            request.Environment.EnvironmentId,
            request.Environment.Name,
            request.Environment.Comment
        };

        await connection.ExecuteAsync(InsertEnvironment, record);

        return Result.Ok();
    }
}