using AutoSpex.Engine;
using Dapper.Contrib.Extensions;

namespace AutoSpex.Persistence;

[Table("Spec")]
public record SpecRecord(
    Guid NodeId,
    Element Element,
    SpecOptionsRecord Options,
    CriterionRecord Range,
    List<CriterionRecord> Filters,
    List<CriterionRecord> Verifications)
{
}