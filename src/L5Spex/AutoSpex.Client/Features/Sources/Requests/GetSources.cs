using System;
using System.Threading;
using System.Threading.Tasks;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;
using Source = AutoSpex.Client.Features.Sources.Models.Source;

namespace AutoSpex.Client.Features.Sources.Requests;

[PublicAPI]
public record GetSourceRequest(Guid NodeId) : IRequest<Result<Source>>;

[UsedImplicitly]
public class GetSourceHandler : IRequestHandler<GetSourceRequest, Result<Source>>
{
    private const string Query = "SELECT * FROM Source WHERE NodeId = @NodeId";
    
    private readonly IDataStoreProvider _dataStore;

    public GetSourceHandler(IDataStoreProvider dataStore)
    {
        _dataStore = dataStore;
    }

    public async Task<Result<Source>> Handle(GetSourceRequest request,
        CancellationToken cancellationToken)
    {
        var connection = await _dataStore.ConnectTo(StoreType.Project, cancellationToken);

        var record = await connection.QuerySingleOrDefaultAsync(Query, new {request.NodeId});
        
        /*Result.FailIf(record is null, $"No source with the provided id '{request.NodeId}' was found.");*/

        var source = new Source(record);

        return Result.Ok(source);
    }
}