using System.Text.Json;
using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record PostRun(Run Run) : ICommandRequest<Result>
{
    public IEnumerable<Change> GetChanges()
    {
        yield return Change.For<PostRun>(Run.Node.NodeId, ChangeType.Created, $"Run posted for {Run.Node.Name}");
        yield return Change.For<PostRun>(Run.Source.SourceId, ChangeType.Created, $"Run posted for {Run.Source.Name}");
    }
}

[UsedImplicitly]
internal class PostRunHandler(IConnectionManager manager) : IRequestHandler<PostRun, Result>
{
    private const string RunExists =
        "SELECT COUNT() FROM Run WHERE RunId = @RunId";

    private const string InsertRun =
        """
        INSERT INTO RUN (RunId, Name, Node, Source, Result, RanOn, RanBy, Duration, PassRate, Outcomes) 
        VALUES (@RunId, @Name, @Node, @Source, @Result, @RanOn, @RanBy, @Duration, @PassRate, @Outcomes)
        """;

    public async Task<Result> Handle(PostRun request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);

        var exists = await connection.QuerySingleAsync<int>(RunExists, new { request.Run.RunId });
        if (exists != 0)
            return Result.Fail($"Run already exists: {request.Run.RunId}");

        using var transaction = connection.BeginTransaction();

        var node = JsonSerializer.Serialize(request.Run.Node);
        var source = JsonSerializer.Serialize(request.Run.Source);
        var outcomes = JsonSerializer.Serialize(request.Run.Outcomes);

        await connection.ExecuteAsync(InsertRun,
            new
            {
                request.Run.RunId,
                request.Run.Name,
                Node = node,
                Source = source,
                request.Run.Result,
                request.Run.RanOn,
                request.Run.RanBy,
                request.Run.Duration,
                request.Run.PassRate,
                Outcomes = outcomes
            },
            transaction);

        transaction.Commit();
        return Result.Ok();
    }
}