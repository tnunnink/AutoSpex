using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using JetBrains.Annotations;
using L5Spex.Client.Observers;
using L5Spex.Client.Services;
using LanguageExt.Common;
using MediatR;

namespace L5Spex.Client.Requests;

public record GetSourcesRequest : IRequest<Result<IEnumerable<SourceObserver>>>;

[UsedImplicitly]
public class GetSourcesHandler : IRequestHandler<GetSourcesRequest, Result<IEnumerable<SourceObserver>>>
{
    private readonly IDatabaseProvider _databaseProvider;

    public GetSourcesHandler(IDatabaseProvider databaseProvider)
    {
        _databaseProvider = databaseProvider;
    }

    public async Task<Result<IEnumerable<SourceObserver>>> Handle(GetSourcesRequest request,
        CancellationToken cancellationToken)
    {
        using var connection = _databaseProvider.Connect();

        var records = (await connection.QueryAsync("SELECT * FROM Source ORDER BY Path DESC")).ToList();

        return new Result<IEnumerable<SourceObserver>>(records.Select(r => new SourceObserver(r)));
    }
}