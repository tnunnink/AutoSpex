using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record GetSpec(Guid NodeId) : IDbQuery<Result<Spec>>;

[UsedImplicitly]
internal class GetSpecHandler(IConnectionManager manager) : IRequestHandler<GetSpec, Result<Spec>>
{
    private const string GetSpec =
        """
        SELECT SpecId, Name, Specification
        FROM Spec S
        JOIN Node N on S.SpecId = N.NodeId
        WHERE SpecId = @SpecId;
        """;

    public async Task<Result<Spec>> Handle(GetSpec request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(Database.Project, cancellationToken);
        
        var record = await connection.QuerySingleOrDefaultAsync(GetSpec,  new { SpecId = request.NodeId });
        if (record is null)
            return Result.Fail<Spec>($"No specification data was found for node '{request.NodeId}'.");

        var node = Node.Create(Guid.Parse(record.SpecId), NodeType.Spec, record.Name.ToString());
        var config = Spec.Deserialize(record.Specification);
        var spec = new Spec(node);
        spec.Update(config);
        return Result.Ok(spec);
    }
}