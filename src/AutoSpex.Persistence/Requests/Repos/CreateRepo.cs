using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record CreateRepo(Repo Repo) : IRequest<Result>;

[UsedImplicitly]
internal class CreateRepoHandler(IConnectionManager manager) : IRequestHandler<CreateRepo, Result>
{
    private const string RepoExists = "SELECT COUNT() FROM Repo WHERE Location = @Location";
    private const string InsertRepo = "INSERT INTO Repo (Location, Name) VALUES (@Location, @Name)";

    public async Task<Result> Handle(CreateRepo request, CancellationToken cancellationToken)
    {
        using var app = await manager.Connect(cancellationToken);

        if (await app.QuerySingleAsync<int>(RepoExists, new { request.Repo.Location }) > 0)
            return Result.Fail($"Repository location '{request.Repo.Location}' already exists");

        await app.ExecuteAsync(InsertRepo, request.Repo);

        return Result.Ok();
    }
}