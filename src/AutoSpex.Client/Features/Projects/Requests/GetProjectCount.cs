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
    private readonly IDataStoreProvider _store;

    public GetProjectCountHandler(IDataStoreProvider store)
    {
        _store = store;
    }

    public async Task<Result<int>> Handle(GetProjectCountRequest request, CancellationToken cancellationToken)
    {
        using var connection = await _store.ConnectTo(StoreType.Application, cancellationToken);
        var result = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Project", cancellationToken);
        return Result.Ok(result);
    }
}