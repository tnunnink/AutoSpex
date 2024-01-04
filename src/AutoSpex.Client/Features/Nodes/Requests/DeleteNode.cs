﻿using System;
using System.Threading;
using System.Threading.Tasks;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Client.Features.Nodes;

[PublicAPI]
public record DeleteNodeRequest(Guid NodeId) : IRequest<Result<Guid>>;

[UsedImplicitly]
public class DeleteNodeHandler : IRequestHandler<DeleteNodeRequest, Result<Guid>>
{
    private readonly ProjectDatabase _database;

    public DeleteNodeHandler(ProjectDatabase database)
    {
        _database = database;
    }

    public async Task<Result<Guid>> Handle(DeleteNodeRequest request, CancellationToken cancellationToken)
    {
        var connection = await _database.Connect(cancellationToken);

        var result = await connection.ExecuteAsync("DELETE FROM Node WHERE NodeId = @NodeId", new {request.NodeId});
        
        return result == 1
            ? Result.Ok(request.NodeId)
            : Result.Fail<Guid>("Failed to delete node");
    }
}