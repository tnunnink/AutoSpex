using L5Sharp.Core;

namespace AutoSpex.Persistence;

public class Source
{
    public Source(L5X content)
    {
        
    }
    public Guid NodeId { get; private init; }

    public string ControllerName { get; }

    public string? Processor { get; }

    public string? Revision { get; }

    public bool HasContext { get; }

    public string? TargetType { get; }

    public string? TargetName { get; }

    public string? ExportedBy { get; }

    public DateTime? ExportedOn { get; }

    public L5X Content { get; }
}