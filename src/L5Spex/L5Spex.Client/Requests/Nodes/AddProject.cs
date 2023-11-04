using System;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using JetBrains.Annotations;
using L5Spex.Client.Common;
using L5Spex.Client.Observers;
using L5Spex.Client.Services;
using LanguageExt.Common;
using MediatR;

namespace L5Spex.Client.Requests.Nodes;

public record AddProjectRequest(string Name) : IRequest<Result<NodeObserver>>;

[UsedImplicitly]
public class AddProjectHandler : IRequestHandler<AddProjectRequest, Result<NodeObserver>>
{
    private const string GetNextOrdinal =
        "SELECT coalesce(MAX(Ordinal) + 1, 0) FROM [Node] WHERE NodeType='Project'";

    private const string InsertNode =
        "INSERT INTO Node (NodeId, ParentId, NodeType, Name, Ordinal, Created, Modified) " +
        "VALUES (@NodeId, @ParentId, @NodeType, @Name, @Ordinal, @Created, @Modified)";

    private readonly IDatabaseProvider _database;

    public AddProjectHandler(IDatabaseProvider database)
    {
        _database = database;
    }

    public async Task<Result<NodeObserver>> Handle(AddProjectRequest request, CancellationToken cancellationToken)
    {
        using var connection = _database.Connect();

        var ordinal = await connection.QuerySingleAsync<int>(GetNextOrdinal);

        var node = new
        {
            NodeId = Guid.NewGuid(),
            ParentId = (Guid?) null,
            NodeType = NodeType.Project.ToString(),
            request.Name,
            Ordinal = ordinal,
            Created = DateTime.Now,
            Modified = DateTime.Now
        };

        await connection.ExecuteAsync(InsertNode, node);

        return new Result<NodeObserver>(new NodeObserver(node));
    }
}