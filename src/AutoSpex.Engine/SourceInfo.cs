using JetBrains.Annotations;

namespace AutoSpex.Engine;

public record SourceInfo
{
    [UsedImplicitly]
    private SourceInfo()
    {
    }

    private SourceInfo(Source source) : this()
    {
        Hash = source.Hash;
        Location = source.Location;
        Name = source.Name;
        UpdatedOn = source.UpdatedOn;
        Size = source.Size;
    }

    public string Hash { get; private init; } = string.Empty;
    public string Location { get; private init; } = string.Empty;
    public string Name { get; private init; } = string.Empty;
    public DateTime UpdatedOn { get; private init; }
    public long Size { get; private init; }
    public static SourceInfo Empty => new();
    public static implicit operator SourceInfo(Source source) => new(source);
}