namespace L5Spex.Persistence.Records;

public record SourceRecord(Guid Id, string Path, bool Selected, bool Pinned);