using System.Collections.Generic;
using System.Linq;
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
public record GetProjectsRequest : IRequest<Result<IEnumerable<Project>>>;

[UsedImplicitly]
public class GetProjectsHandler : IRequestHandler<GetProjectsRequest, Result<IEnumerable<Project>>>
{
    private readonly IDataStoreProvider _dataStore;

    public GetProjectsHandler(IDataStoreProvider dataStore)
    {
        _dataStore = dataStore;
    }
    
    public async Task<Result<IEnumerable<Project>>> Handle(GetProjectsRequest request, CancellationToken cancellationToken)
    {
        using var connection = await _dataStore.ConnectTo(StoreType.Application, cancellationToken);
        var records = await connection.QueryAsync("SELECT * FROM Project ORDER BY OpenedOn DESC");
        return Result.Ok(records.Select(record => new Project(record)));
    }
}