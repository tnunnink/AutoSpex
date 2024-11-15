using L5Sharp.Core;

namespace AutoSpex.Engine;

public class Reference
{
    public Guid SourceId { get; private set; } = Guid.Empty;
    
    public Scope Scope { get; private set; } = string.Empty;

    public LogixElement? Value { get; private set; }
}