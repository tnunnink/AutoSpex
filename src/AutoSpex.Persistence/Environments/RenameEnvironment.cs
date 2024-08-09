using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;
using Environment = AutoSpex.Engine.Environment;

namespace AutoSpex.Persistence;

[PublicAPI]
public record RenameEnvironment(Environment Environment) : IDbCommand<Result>;

[UsedImplicitly]
internal class RenameEnvironmentHandler(IConnectionManager manager) : IRequestHandler<RenameEnvironment, Result>
{
    private const string RenameEnvironment =
        "UPDATE Environment SET Name = @Name WHERE EnvironmentId = @EnvironmentId";

    public async Task<Result> Handle(RenameEnvironment request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        var updated = await connection.ExecuteAsync(RenameEnvironment,
            new { request.Environment.EnvironmentId, request.Environment.Name });
        return Result.OkIf(updated == 1, $"Environment not found: '{request.Environment.EnvironmentId}'");
    }
}