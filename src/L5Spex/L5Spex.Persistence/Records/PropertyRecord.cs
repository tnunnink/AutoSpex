namespace L5Spex.Persistence.Records;

public record PropertyRecord(Guid PropertyId, Guid TypeId, Guid ParentTypeId, string PropertyName);