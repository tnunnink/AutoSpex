using Dapper.Contrib.Extensions;

namespace AutoSpex.Persistence;

[Table("Argument")]
public record ArgumentRecord(
    Guid ArgumentId,
    Guid CriterionId,
    Type Type,
    string Data);