using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record CreateRunner(Runner Runner) : ICommand<Result>;

[UsedImplicitly]
internal class CreateRunnerHandler(IConnectionManager manager) : IRequestHandler<CreateRunner, Result>
{
    private const string InsertRunner =
        "INSERT INTO Runner (RunnerId, Name, Description) VALUES (@RunnerId, @Name, @Description);";
    
    public async Task<Result> Handle(CreateRunner request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(Database.Project, cancellationToken);
        await connection.ExecuteAsync(InsertRunner, request.Runner);
        return Result.Ok();
    }
}