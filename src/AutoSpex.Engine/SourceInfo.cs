using JetBrains.Annotations;
using L5Sharp.Core;

namespace AutoSpex.Engine;

public record SourceInfo
{
    [UsedImplicitly]
    private SourceInfo()
    {
    }

    public SourceInfo(L5X content)
    {
        Name = content.Info.TargetName!;
        Type = content.Info.TargetType!;
        Revision = content.Info.SoftwareRevision!;
        ExportedOn = content.Info.ExportDate?.ToString()!;
        Owner = content.Info.Owner!;
    }

    /// <summary>
    /// The name of the element that is the target of the export.
    /// </summary>
    public string Name { get; private init; } = string.Empty;

    /// <summary>
    /// The type of the element that is the target of the export.
    /// </summary>
    public string Type { get; private init; } = string.Empty;

    /// <summary>
    /// The software version that exported the L5X content this source represents.
    /// </summary>
    public string Revision { get; private init; } = string.Empty;

    /// <summary>
    /// The date/time that the content was exported.
    /// </summary>
    public string ExportedOn { get; private init; } = string.Empty;

    /// <summary>
    /// The name of the user that exported the content.
    /// </summary>
    public string Owner { get; private init; } = string.Empty;

    /// <summary>
    /// Represents an empty instance of the SourceInfo class.
    /// </summary>
    /// This property is used to provide an empty instance of SourceInfo for initialization purposes.
    public static SourceInfo Empty => new();

    /// <summary>
    /// Represents the source information extracted from L5X content.
    /// </summary>
    public static implicit operator SourceInfo(L5X content) => new(content);
}