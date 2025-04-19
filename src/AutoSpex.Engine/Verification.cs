namespace AutoSpex.Engine;

/// <summary>
/// An object that will aggregate all evaluations that were executed for a single candidate object.
/// </summary>
public class Verification
{
    /// <summary>
    /// An object that will aggregate all evaluations that were executed for a single candidate object.
    /// </summary>
    public Verification(object? candidate, Evaluation[] evaluations)
    {
        Result = ResultState.MaxOrDefault(evaluations.Select(e => e.Result).ToList());
        Candidate = candidate?.Dereference();
        Evaluations = evaluations;
    }
    
    /// <summary>
    /// An object that will aggregate all evaluations that were executed for a single candidate object.
    /// </summary>
    public Verification(ResultState result, object? candidate = null)
    {
        Result = result;
        Candidate = candidate?.Dereference();
        Evaluations = [];
    }

    /// <summary>
    /// Represents the final aggregated result of the verification process, derived from
    /// the results of all <see cref="Evaluation"/> instances associated with the verification.
    /// This property evaluates to the most significant <see cref="ResultState"/> based on all evaluations.
    /// </summary>
    public ResultState Result { get; }

    /// <summary>
    /// Represents the candidate object that is being tested or verified within the context of the verification.
    /// This serves as the subject for which the <see cref="Evaluation"/> instances are generated and assessed.
    /// </summary>
    public object? Candidate { get; }

    /// <summary>
    /// The collection of <see cref="Evaluation"/> that belongs to the verification.
    /// These represent the checks produced by a spec object and grouped together for a single candidate.
    /// </summary>
    public IReadOnlyCollection<Evaluation> Evaluations { get; }
}