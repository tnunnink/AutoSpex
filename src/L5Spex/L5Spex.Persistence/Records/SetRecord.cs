namespace L5Spex.Persistence.Records;

public record SetRecord(Guid SetId, Guid ParentId, string Name, string Description, bool Enabled, DateTime Modified);