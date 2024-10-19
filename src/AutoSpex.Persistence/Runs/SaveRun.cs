using System.Text.Json;
using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record SaveRun(Run Run) : IDbCommand<Result>;

[UsedImplicitly]
internal class SaveRunHandler(IConnectionManager manager) : IRequestHandler<SaveRun, Result>
{
    private const string RunExists =
        "SELECT COUNT() FROM Run WHERE RunId = @RunId";

    private const string InsertRun =
        """
        INSERT INTO RUN (RunId, Name, Node, Source, Result, RanOn, RanBy, Outcomes) 
        VALUES (@RunId, @Name, @Node, @Source, @Result, @RanOn, @RanBy, @Outcomes)
        """;

    public async Task<Result> Handle(SaveRun request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);

        var exists = await connection.QuerySingleAsync<int>(RunExists, new { request.Run.RunId });
        if (exists != 0)
            return Result.Fail($"Run already exists: {request.Run.RunId}");

        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(InsertRun,
            new
            {
                request.Run.RunId,
                request.Run.Name,
                Node = JsonSerializer.Serialize(request.Run.Node),
                Source = JsonSerializer.Serialize(request.Run.Source),
                request.Run.Result,
                request.Run.RanOn,
                request.Run.RanBy,
                Outcomes = JsonSerializer.Serialize(request.Run.Outcomes)
            },
            transaction);

        transaction.Commit();
        return Result.Ok();
    }
}