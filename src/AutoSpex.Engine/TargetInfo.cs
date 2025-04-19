using L5Sharp.Core;

namespace AutoSpex.Engine;

public record TargetInfo(string Name, string Type, string Revision, DateTime ModifiedOn, string Owner)
{
    public static TargetInfo Empty => new(string.Empty, string.Empty, string.Empty, DateTime.UnixEpoch, string.Empty);

    public static implicit operator TargetInfo(L5X content) => FromContent(content);

    private static TargetInfo FromContent(L5X content)
    {
        var name = content.Info.TargetName ?? string.Empty;
        var type = content.Info.TargetType ?? string.Empty;
        var revision = content.Info.SoftwareRevision ?? string.Empty;
        var modified = content.Controller.LastModifiedDate;
        var owner = content.Info.Owner ?? string.Empty;

        return new TargetInfo(name, type, revision, modified, owner);
    }
}