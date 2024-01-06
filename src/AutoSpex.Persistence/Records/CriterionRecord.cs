using AutoSpex.Engine;
using Dapper.Contrib.Extensions;

namespace AutoSpex.Persistence;

[Table("Criterion")]
public record CriterionRecord(
    Guid CriterionId,
    Guid NodeId,
    CriterionUsage Usage,
    string Property,
    Operation Operation,
    List<ArgumentRecord> Arguments)
{
};