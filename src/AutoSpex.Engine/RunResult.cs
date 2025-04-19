namespace AutoSpex.Engine;

/// <summary>
/// A result summary object that contains the high-level information about the result of a run.
/// This object will be the light-weight record that we persist and can re-run again in the future.
/// </summary>
public class RunResult
{
    /// <summary>
    /// The node that was run to produce this result.
    /// </summary>
    public NodeInfo Node { get; init; } = NodeInfo.Empty;

    /// <summary>
    /// The source that the node was run against to produce this result.
    /// </summary>
    public SourceInfo Source { get; init; } = SourceInfo.Empty;

    /// <summary>
    /// The info of that source target that produced this run (additional top level L5X info separate from source file info)
    /// </summary>
    public TargetInfo Target { get; init; } = TargetInfo.Empty;

    /// <summary>
    /// The result state of the run.
    /// </summary>
    public ResultState Result { get; init; } = ResultState.None;

    /// <summary>
    /// The time (ms) that it took for the run to complete.
    /// </summary>
    public long Duration { get; init; }

    /// <summary>
    /// The total number of specifications that this run evaluated.
    /// </summary>
    public int Total { get; init; }

    /// <summary>
    /// The total number of specifications that passed verification.
    /// </summary>
    public int Passed { get; init; }

    /// <summary>
    /// The total number of specifications that failed verification.
    /// </summary>
    public int Failed { get; init; }

    /// <summary>
    /// The total number of specifications that resulted in an error.
    /// </summary>
    public int Errored { get; init; }

    /// <summary>
    /// The date/time that this run result was produced.
    /// </summary>
    public DateTime RanOn { get; private init; } = DateTime.Now;

    /// <summary>
    /// The user that produces this run result.
    /// </summary>
    public string RanBy { get; private init; } = Environment.UserName;
}