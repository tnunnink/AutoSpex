namespace AutoSpex.Engine;

/// <summary>
/// An object containing the resulting data from running a <see cref="Spec"/> against a given <see cref="Source"/>.
/// </summary>
public class Outcome
{
    private readonly List<Verification> _verifications = [];

    public Outcome(Node node)
    {
        ArgumentNullException.ThrowIfNull(node);
        NodeId = node.NodeId;
        Name = node.Name;
    }

    /// <summary>
    /// The <see cref="Guid"/> that uniquely represents this outcome object.
    /// </summary>
    public Guid OutcomeId { get; private init; } = Guid.NewGuid();

    /// <summary>
    /// The id of the node that this outcome represents. 
    /// </summary>
    public Guid NodeId { get; private init; }

    /// <summary>
    /// The name of the spec that this outcome represents.
    /// </summary>
    public string Name { get; private init; }

    /// <summary>
    /// The result of the outcome. This represents if the spec passed, failed, errored, etc.
    /// </summary>
    public ResultState Result { get; private set; } = ResultState.None;

    /// <summary>
    /// Represents the duration of running a spec against a given source or collection of sources.
    /// </summary>
    public long Duration { get; private set; }

    /// <summary>
    /// The evaluations associated with the Outcome object.
    /// </summary>
    /// <remarks>
    /// This property represents the evaluations performed by the Verifications associated with the Outcome object. Evaluations are generated based on the criteria defined in the Spec and the data from the Source.
    /// </remarks>
    public IEnumerable<Evaluation> Evaluations => _verifications.SelectMany(v => v.Evaluations);

    /// <summary>
    /// Adds a verification to the collection of verifications for the outcome and updates the local result state.
    /// </summary>
    /// <param name="verification">The verification to be added.</param>
    public void Add(Verification verification)
    {
        ArgumentNullException.ThrowIfNull(verification);
        _verifications.Add(verification);
        UpdateResult();
    }

    /// <summary>
    /// Removes a verification from the collection of verifications for the outcome and updates the local result state.
    /// </summary>
    /// <param name="verification">The verification to be removed.</param>
    public void Reomve(Verification verification)
    {
        ArgumentNullException.ThrowIfNull(verification);
        _verifications.Remove(verification);
        UpdateResult();
    }

    /// <summary>
    /// Clears the collection of verifications for the outcome and updates the local result state.
    /// </summary>
    public void Clear()
    {
        _verifications.Clear();
        UpdateResult();
    }

    /// <summary>
    /// Updates the local result state of the outcome using the current collection of verifications.
    /// </summary>
    private void UpdateResult()
    {
        Result = ResultState.MaxOrDefault(_verifications.Select(v => v.Result).ToList());
        Duration = _verifications.Sum(v => v.Duration);
    }
}