using JetBrains.Annotations;
using L5Sharp.Core;

namespace AutoSpex.Engine;

[PublicAPI]
public class Source
{
    private L5X? _l5X;
    
    [UsedImplicitly]
    private Source()
    {
    }

    public Source(L5X l5X)
    {
        _l5X = l5X ?? throw new ArgumentNullException(nameof(l5X));

        Name = _l5X.Info.TargetName ?? "New Source";
        Description = _l5X.Controller.Description ?? string.Empty;
        TargetType = _l5X.Info.TargetType ?? string.Empty;
        TargetName = _l5X.Info.TargetName ?? string.Empty;
        ExportedOn = _l5X.Info.ExportDate ?? DateTime.Today;
        ExportedBy = _l5X.Info.Owner ?? string.Empty;
        Content = _l5X.ToString().Compress();
    }

    public Guid SourceId { get; private init; } = Guid.NewGuid();
    public bool IsSelected { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TargetType { get; private set; } = string.Empty;
    public string TargetName { get; private set; } = string.Empty;
    public DateTime ExportedOn { get; private set; } = DateTime.Now;
    public string ExportedBy { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public L5X L5X => _l5X ??= L5X.Parse(Content.Decompress());

    public void Update(L5X l5X)
    {
        _l5X = l5X ?? throw new ArgumentNullException(nameof(l5X));
        TargetType = _l5X.Info.TargetType ?? string.Empty;
        TargetName = _l5X.Info.TargetName ?? string.Empty;
        ExportedOn = _l5X.Info.ExportDate ?? DateTime.Today;
        ExportedBy = _l5X.Info.Owner ?? string.Empty;
        Content = _l5X.ToString().Compress();
    }
}