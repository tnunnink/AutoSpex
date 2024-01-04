using System.Threading;
using System.Threading.Tasks;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using CommunityToolkit.Mvvm.Messaging;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Client.Features.Nodes;

[PublicAPI]
public record RenameNodeRequest(Node Node) : IRequest<Result>;

[UsedImplicitly]
public class RenameNodeHandler : IRequestHandler<RenameNodeRequest, Result>
{
    private readonly ProjectDatabase _connector;
    private readonly IMessenger _messenger;

    public RenameNodeHandler(ProjectDatabase connector, IMessenger messenger)
    {
        _connector = connector;
        _messenger = messenger;
    }

    public async Task<Result> Handle(RenameNodeRequest request, CancellationToken cancellationToken)
    {
        var node = request.Node;
        
        var connection = await _connector.Connect(cancellationToken);

        var modified = await connection.ExecuteAsync("UPDATE Node SET Name = @Name WHERE NodeId = @NodeId",
            new {node.NodeId, node.Name});

        var result = modified == 1
            ? Result.Ok()
            : Result.Fail("Failed to rename node"); //todo obviously need to think what is the best way to handle failed requests
        
        if (result.IsSuccess)
        {
            _messenger.Send(new NodeRenamedMessage(node));
        }

        return result;
    }
}