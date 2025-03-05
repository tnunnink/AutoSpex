using System.Text.Json.Serialization;
using Ardalis.SmartEnum.SystemTextJson;

namespace AutoSpex.Engine;

/// <summary>
/// An object containing the resulting data from running a <see cref="Spec"/> against a given <see cref="Source"/>.
/// </summary>
public class Outcome
{
    /// <summary>
    /// The collection of <see cref="Evaluation"/> that this outcome contains.
    /// </summary>
    private List<Evaluation> _evaluations = [];

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
    /// The path of the spec that this outcome represents.
    /// </summary>
    [JsonInclude]
    public string Path { get; init; } = string.Empty;

    /// <summary>
    /// The aggregate result of the outcome.
    /// </summary>
    [JsonInclude]
    [JsonConverter(typeof(SmartEnumNameConverter<ResultState, int>))]
    public ResultState Result { get; private set; } = ResultState.None;

    /// <summary>
    /// The total time it took to execute the spec that generated this outcome. 
    /// </summary>
    [JsonInclude]
    public long Duration { get; private set; }

    /// <summary>
    /// The rate or percentage of total evaluations that passed for this outcome.
    /// </summary>
    [JsonInclude]
    public double PassRate { get; private set; }

    /// <summary>
    /// The collection of <see cref="Evaluation"/> results that this outcome contains by having run a spec against a source.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyCollection<Evaluation> Evaluations => _evaluations;

    /// <summary>
    /// Applies the verification result to the Outcome object by setting the Result, Duration, PassRate, and Evaluations.
    /// </summary>
    /// <param name="verification">The Verification object containing the result to apply.</param>
    public void Apply(Verification verification)
    {
        ArgumentNullException.ThrowIfNull(verification);

        Result = verification.Result;
        Duration = verification.Duration;
        PassRate = verification.PassRate;

        _evaluations = verification.Evaluations.ToList();
    }
}