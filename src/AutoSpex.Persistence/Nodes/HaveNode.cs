using AutoSpex.Engine;
using Dapper;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record HaveNode(string Name, NodeType Type) : IRequest<bool>;

[UsedImplicitly]
internal class CollectionExistsHandler(IConnectionManager manager) : IRequestHandler<HaveNode, bool>
{
    private const string Exists = "SELECT COUNT() FROM Node WHERE Type = @Type AND Name = @Name";

    public async Task<bool> Handle(HaveNode request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        var result = await connection.QuerySingleAsync<int>(Exists, new { request.Type, request.Name });
        return result > 0;
    }
}