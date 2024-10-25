using System.Text.Json.Serialization;

namespace AutoSpex.Engine;

/// <summary>
/// An object containing the resulting data from running a <see cref="Spec"/> against a given <see cref="Source"/>.
/// </summary>
public class Outcome
{
    /// <summary>
    /// The <see cref="Guid"/> that uniquely represents this outcome object.
    /// </summary>
    [JsonInclude]
    public Guid OutcomeId { get; private init; } = Guid.NewGuid();

    /// <summary>
    /// The id of the run that this outcome belongs to. 
    /// </summary>
    [JsonInclude]
    public Guid RunId { get; init; } = Guid.Empty;

    /// <summary>
    /// The id of the node that this outcome represents. 
    /// </summary>
    [JsonInclude]
    public Guid NodeId { get; init; } = Guid.Empty;

    /// <summary>
    /// The name of the spec that this outcome represents.
    /// </summary>
    [JsonInclude]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The <see cref="Engine.Verification"/> that contains the result data produced by running the corresponding node.
    /// </summary>
    [JsonInclude]
    public Verification Verification { get; set; } = Verification.None;

    /// <summary>
    /// A supporession message that explains why this outcome is not applicable to necessary for the run it was a part of.
    /// This allows user to ignore certain specs/outcomes as part of a run that don't apply to a certain source/project.
    /// </summary>
    [JsonInclude]
    public string? Suppression { get; set; }

    /// <summary>
    /// Suppresses the current verification, setting the ResultState to Suppressed while keeping the
    /// original evaluations and duration.
    /// </summary>
    /// <param name="reason">The reason for suppressing the verification.</param>
    public void Suppress(string reason)
    {
        Verification = Verification.Suppressed;
        Suppression = reason;
    }
}