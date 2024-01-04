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
    private const string Query = "SELECT * FROM Project ORDER BY OpenedOn DESC";

    private readonly AppDatabase _database;

    public GetProjectsHandler(AppDatabase database)
    {
        _database = database;
    }
    
    public async Task<Result<IEnumerable<Project>>> Handle(GetProjectsRequest request, CancellationToken cancellationToken)
    {
        var connection = await _database.Connect(cancellationToken);
        var projects = await connection.QueryAsync<Project>(Query);
        return Result.Ok(projects);
    }
}