namespace AutoSpex.Engine;

/// <summary>
/// Represents the result of a run operation executed on a specific node.
/// </summary>
public class RunResult
{
    public RunResult(params Node[] nodes)
    {
        Result = ResultState.MaxOrDefault(nodes.Select(n => n.Verification.Result).ToArray());
        RanOn = DateTime.Now;
        RanBy = Environment.UserName;
        Duration = nodes.Sum(n => n.Verification.Duration);
    }
    
    public ResultState Result { get; }
    public DateTime RanOn { get; }
    public string RanBy { get; }
    public long Duration { get; }
}