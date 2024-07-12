using System.Text.Json;
using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record PostRun(Run Run) : IDbCommand<Result>;

[UsedImplicitly]
internal class PostRunHandler(IConnectionManager manager) : IRequestHandler<PostRun, Result>
{
    private const string EnvironmentExists =
        "SELECT COUNT() FROM Environment WHERE EnvironmentId = @EnvironmentId";

    private const string PostRun =
        """
        INSERT INTO Run (RunId, EnvironmentId, Result, RanOn, RanBy, Outcomes)
        VALUES (@RunId, @EnvironmentId, @Result, @RanOn, @RanBy, @Outcomes)
        """;

    public async Task<Result> Handle(PostRun request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);

        var id = request.Run.Environment.EnvironmentId;
        var exists = await connection.QuerySingleAsync<int>(EnvironmentExists, new { EnvironmentId = id });
        if (exists != 1) return Result.Fail($"Environment not found: {request.Run.Environment.Name}");

        var run = new
        {
            RunId = Guid.NewGuid(),
            request.Run.Environment.EnvironmentId,
            request.Run.Result,
            request.Run.RanOn,
            request.Run.RanBy,
            Outcomes = JsonSerializer.Serialize(request.Run.Outcomes)
        };

        await connection.ExecuteAsync(PostRun, run);
        return Result.Ok();
    }
}