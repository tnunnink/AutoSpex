namespace AutoSpex.Engine;

public class Verification
{
    /// <summary>
    /// Gets the unique identifier of the verification.
    /// </summary>
    public Guid VerificationId { get; } = Guid.NewGuid();

    /// <summary>
    /// The information of the node that produced the verification.
    /// </summary>
    public NodeInfo Node { get; private set; } = NodeInfo.Empty;

    /// <summary>
    /// The information of the source that produced the verification.
    /// </summary>
    public SourceInfo Source { get; private set; } = SourceInfo.Empty;

    /// <summary>
    /// The total result for all evaluations of this verification, indicating whether a spec Passed/Failed/Errored.
    /// </summary>
    public ResultState Result { get; private set; } = ResultState.None;

    /// <summary>
    /// The duration or time it took for all evaluations of this verification to process.
    /// </summary>
    public long Duration { get; private set; }

    /// <summary>
    /// The collection of <see cref="Evaluation"/> that belong to the verification.
    /// These represent the checks produced by a spec object and grouped together to form this single verification.
    /// </summary>
    public IReadOnlyCollection<Evaluation> Evaluations { get; private set; } = [];


    /// <summary>
    /// Marks the verification as pending with the provided source information.
    /// </summary>
    /// <param name="node">The node for which verification is pending.</param>
    /// <param name="source">The source for which this verification is pending.</param>
    /// <param name="callback">A callback to invoke when the result state changes.</param>
    public void MarkPending(NodeInfo node, SourceInfo source, Action<Verification>? callback = null)
    {
        Node = node;
        Source = source;
        Result = ResultState.Pending;
        Duration = 0;
        Evaluations = [];

        callback?.Invoke(this);
    }

    /// <summary>
    /// Marks the verification as currently running.
    /// </summary>
    /// <param name="callback">A callback to invoke when the result state changes.</param>
    public void MarkRunning(Action<Verification>? callback = null)
    {
        Result = ResultState.Running;

        callback?.Invoke(this);
    }

    /// <summary>
    /// Marks the verification as complete with the provided evaluations and duration.
    /// </summary>
    /// <param name="evaluations">The array of evaluations to set for the verification.</param>
    /// <param name="duration">The duration of the verification process.</param>
    /// <param name="callback">A callback to invoke when the result state changes.</param>
    public void MarkComplete(Evaluation[] evaluations, long duration, Action<Verification>? callback = null)
    {
        Result = ResultState.MaxOrDefault(evaluations.Select(e => e.Result).ToList());
        Duration = duration;
        Evaluations = evaluations;

        callback?.Invoke(this);
    }

    /// <summary>
    /// Marks the verification as complete with the provided child verifications for which to summarize.
    /// </summary>
    /// <param name="verifications">The array of verification to summarize for this aggregate verification.</param>
    /// <param name="callback">A callback to invoke when the result state changes.</param>
    public void MarkComplete(Verification[] verifications, Action<Verification>? callback = null)
    {
        Result = ResultState.MaxOrDefault(verifications.Select(v => v.Result).ToList());
        Duration = verifications.Sum(v => v.Duration);

        callback?.Invoke(this);
    }
}