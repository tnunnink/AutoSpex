using System.Threading;
using System.Threading.Tasks;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Client.Features.Projects;

[PublicAPI]
public record GetProjectCountRequest : IRequest<Result<int>>;

[UsedImplicitly]
public class GetProjectCountHandler : IRequestHandler<GetProjectCountRequest, Result<int>>
{
    private readonly AppDatabase _database;

    public GetProjectCountHandler(AppDatabase database)
    {
        _database = database;
    }

    public async Task<Result<int>> Handle(GetProjectCountRequest request, CancellationToken cancellationToken)
    {
        var connection = await _database.Connect(cancellationToken);
        var result = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Project", cancellationToken);
        return Result.Ok(result);
    }
}