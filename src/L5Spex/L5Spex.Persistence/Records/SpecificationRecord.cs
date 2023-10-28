namespace L5Spex.Persistence.Records;

public record SpecificationRecord(Guid SpecId, Guid SetId, Guid TypeId, 
    string Name, string Description, bool Enabled,
    DateTime Modified);