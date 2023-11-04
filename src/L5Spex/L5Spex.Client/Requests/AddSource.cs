using System;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using JetBrains.Annotations;
using L5Spex.Client.Observers;
using L5Spex.Client.Services;
using LanguageExt.Common;
using MediatR;

namespace L5Spex.Client.Requests;

public record AddSourceRequest(string Path) : IRequest<Result<SourceObserver>>;

[UsedImplicitly]
public class AddSourceHandler : IRequestHandler<AddSourceRequest, Result<SourceObserver>>
{
    private const string ExistsSql = "SELECT EXISTS(SELECT 1 FROM Source WHERE Path = @Path)";
    private const string InsertSource = "INSERT INTO Source (SourceId, Path) VALUES (@SourceId, @Path)";
    
    private readonly IDatabaseProvider _databaseProvider;

    public AddSourceHandler(IDatabaseProvider databaseProvider)
    {
        _databaseProvider = databaseProvider;
    }

    public async Task<Result<SourceObserver>> Handle(AddSourceRequest request, CancellationToken cancellationToken)
    {
        using var connection = _databaseProvider.Connect();

        var exists = await connection.ExecuteScalarAsync<bool>(ExistsSql, new {request.Path});
        if (exists) return new Result<SourceObserver>();

        var source = new {SourceId = Guid.NewGuid(), request.Path};

        await connection.ExecuteAsync(InsertSource, source);

        return new Result<SourceObserver>(new SourceObserver(source));
    }
}