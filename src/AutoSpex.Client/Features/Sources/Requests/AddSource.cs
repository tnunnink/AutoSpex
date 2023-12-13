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

public record AddSourceRequest(Uri Path, string Name, Guid? ParentId = default)
    : AddNodeRequest(Name, NodeType.Source, ParentId);

[UsedImplicitly]
public class AddSourceHandler : AddNodeHandler, IRequestHandler<AddSourceRequest, Result<Node>>
{
    private const string InsertSource = "INSERT INTO Source (NodeId, Controller, Processor, Revision, IsContext, TargetType, TargetName, ExportedBy, ExportedOn, Content) " +
                                        "VALUES (@NodeId, @Controller, @Processor, @Revision, @IsContext, @TargetType, @TargetName, @ExportedBy, @ExportedOn, @Content)";
    
    public AddSourceHandler(IDataStoreProvider dataStore) : base(dataStore)
    {
    }

    public async Task<Result<Node>> Handle(AddSourceRequest request, CancellationToken cancellationToken)
    {
        var content = await L5X.LoadAsync(request.Path.AbsolutePath, cancellationToken);

        var nodeResult = await base.Handle(request, cancellationToken);
        if (nodeResult.IsFailed)
        {
            return Result.Fail("Could not add node to the data base.").WithErrors(nodeResult.Errors);
        }
        
        var source = new
        {
            nodeResult.Value.NodeId,
            Controller = content.Controller.Name,
            Processor = content.Controller.ProcessorType,
            Revision = content.Controller.Revision?.ToString(),
            IsContext = content.Info.ContainsContext == true,
            content.Info.TargetType,
            content.Info.TargetName,
            ExportedBy = content.Info.Owner,
            ExportedOn = content.Info.ExportDate,
            Content = content.ToString()
        };
        
        using var connection = await Store.ConnectTo(StoreType.Project, cancellationToken);
        await connection.ExecuteAsync(InsertSource, source);
        return Result.Ok(nodeResult.Value);
    }
}