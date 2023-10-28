namespace L5Spex.Persistence.Records;

public record CriterionRecord(Guid CriterionId, Guid SpecId, Guid TypeId, Guid PropertyId, Guid OperationId,
    string Values, string Usage, string ChainType, DateTime Modified);