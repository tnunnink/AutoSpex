using System;
using System.Threading;
using System.Threading.Tasks;
using AutoSpex.Client.Features.Nodes;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using L5Sharp.Core;
using MediatR;
using Node = AutoSpex.Client.Features.Nodes.Node;

namespace AutoSpex.Client.Features.Sources.Requests;

public record AddSourceRequest(Uri Path, string Name, Guid ParentId = default) : IRequest<Result<Node>>;

[UsedImplicitly]
public class AddSourceHandler : IRequestHandler<AddSourceRequest, Result<Node>>
{
    private readonly ProjectDatabase _database;
    
    private const string GetNextOrdinal =
        "SELECT coalesce(MAX(Ordinal) + 1, 0) FROM [Node] WHERE ParentId = @ParentId AND NodeType = @NodeType";

    private const string GetParent = "SELECT * FROM [Node] WHERE NodeId = @ParentId";

    private const string InsertNode =
        "INSERT INTO Node (NodeId, ParentId, Feature, NodeType, Name, Depth, Ordinal, Description) " +
        "VALUES (@NodeId, @ParentId, @Feature, @NodeType, @Name, @Depth, @Ordinal, @Description)";

    private const string InsertSource = "INSERT INTO Source (SourceId, Controller, Processor, Revision, IsContext, TargetType, TargetName, ExportedBy, ExportedOn, Content) " +
                                        "VALUES (@NodeId, @Controller, @Processor, @Revision, @IsContext, @TargetType, @TargetName, @ExportedBy, @ExportedOn, @Content)";
    
    public AddSourceHandler(ProjectDatabase database)
    {
        _database = database;
    }

    public async Task<Result<Node>> Handle(AddSourceRequest request, CancellationToken cancellationToken)
    {
        var content = await L5X.LoadAsync(request.Path.AbsolutePath, cancellationToken);
        throw new NotImplementedException();
    }
}