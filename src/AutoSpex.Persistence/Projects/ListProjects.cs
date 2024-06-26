﻿using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

/// <summary>
/// Retrieves all <see cref="Project"/> stored in the local application database.
/// </summary>
[PublicAPI]
public record ListProjects : IDbQuery<Result<IEnumerable<Project>>>;

[UsedImplicitly]
internal class ListProjectsHandler(IConnectionManager manager)
    : IRequestHandler<ListProjects, Result<IEnumerable<Project>>>
{
    private const string ListProjects = "SELECT Path, OpenedOn, Pinned FROM Project ORDER BY OpenedOn DESC";

    public async Task<Result<IEnumerable<Project>>> Handle(ListProjects request,
        CancellationToken cancellationToken)
    {
        var connection = await manager.Connect(Database.App, cancellationToken);
        var projects = await connection.QueryAsync<Project>(ListProjects);
        return Result.Ok(projects);
    }
}