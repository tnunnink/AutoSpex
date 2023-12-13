using System;
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
public record RemoveProjectRequest(Uri Path) : IRequest<Result>;

public class RemoveProjectHandler : IRequestHandler<RemoveProjectRequest, Result>
{
    private const string Command = "DELETE FROM Project WHERE Path = @Path";
    
    private readonly IDataStoreProvider _dataStore;

    public RemoveProjectHandler(IDataStoreProvider dataStore)
    {
        _dataStore = dataStore;
    }
    
    public async Task<Result> Handle(RemoveProjectRequest request, CancellationToken cancellationToken)
    {
        var connection = await _dataStore.ConnectTo(StoreType.Application, cancellationToken);
        var affected = await connection.ExecuteAsync(Command, new {Path = request.Path.LocalPath});
        return Result.Ok().WithSuccess($"Removed {affected} project(s) from application store.");
    }
}