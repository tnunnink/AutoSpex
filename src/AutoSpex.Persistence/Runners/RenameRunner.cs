using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record RenameRunner(Runner Runner) : IDbCommand<Result>;

[UsedImplicitly]
internal class RenameRunnerHandler(IConnectionManager manager) : IRequestHandler<RenameRunner, Result>
{
    private const string Command = "UPDATE Node SET Name = @Name WHERE NodeId = @NodeId";
    
    public async Task<Result> Handle(RenameRunner request, CancellationToken cancellationToken)
    {
        var connection = await manager.Connect(Database.Project, cancellationToken);
        var updated = await connection.ExecuteAsync(Command, request.Runner);
        return Result.OkIf(updated == 1, $"Runner not found: '{request.Runner.RunnerId}'");
    }
}