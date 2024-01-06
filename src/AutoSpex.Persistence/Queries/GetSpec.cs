using System.Text.Json;
using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record GetSpec(Guid NodeId) : IQuery<Result<Specification>>;

public class LoadSpecHandler(IConnectionManager manager, JsonSerializerOptions options)
    : IRequestHandler<GetSpec, Result<Specification>>
{
    private const string GetSpec =
        "SELECT NodeId, Element FROM Spec WHERE SpecId = @SpecId";

    private const string GetCriteria =
        "SELECT * FROM Criterion c JOIN Argument a on a.CriterionId = c.CriterionId WHERE NodeId = @NodeId";

    public async Task<Result<Specification>> Handle(GetSpec request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(Database.Project, cancellationToken);

        var record = await connection.QuerySingleAsync<SpecRecord>(GetSpec, new {request.NodeId});
        var spec = Specification.For(record.Element);

        var children = new Dictionary<Guid, Criterion>();
        await connection.QueryAsync<CriterionRecord, ArgumentRecord, Criterion>(
            GetCriteria,
            (c, a) =>
            {
                if (!children.TryGetValue(c.CriterionId, out var criterion))
                {
                    criterion = new Criterion(c.Property, c.Operation);
                    children.Add(c.CriterionId, criterion);

                    c.Usage
                        .When(CriterionUsage.Filter).Then(() => spec.Filter(criterion))
                        .When(CriterionUsage.Verification).Then(() => spec.Verify(criterion));
                }

                var value = JsonSerializer.Deserialize(a.Data, a.Type, options)!;
                var argument = new Argument(value);
                criterion.Arguments.Add(argument);

                return criterion;
            },
            splitOn: "ArgumentId",
            param: new {request.NodeId});

        return Result.Ok(spec);
    }
}