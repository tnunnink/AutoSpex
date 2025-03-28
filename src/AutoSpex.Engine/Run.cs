namespace AutoSpex.Engine;

/// <summary>
/// A <see cref="Run"/> represents a set of specs and source which should be executed together and produce a set of
/// outcomes or results. 
/// </summary>
public class Run
{
    public NodeInfo Node { get; private set; } = NodeInfo.Empty;
    public SourceInfo Source { get; private set; } = SourceInfo.Empty;
    public ResultState Result { get; private set; } = ResultState.None;
    public DateTime RanOn { get; private set; }
    public string RanBy { get; private set; } = string.Empty;
    public long Duration { get; private set; }
}