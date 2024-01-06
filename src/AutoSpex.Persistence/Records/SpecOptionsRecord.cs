using AutoSpex.Engine;
using Dapper.Contrib.Extensions;

namespace AutoSpex.Persistence;

[Table("SpecOptions")]
public record SpecOptionsRecord(
    Guid NodeId,
    bool RunToEnd,
    InclusionType FilterInclusion
);