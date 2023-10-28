namespace L5Spex.Persistence.Records;

public record SourceRecord(Guid SourceId, string Path, bool Selected, bool Pinned, DateTime Modified);